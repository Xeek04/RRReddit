using LoginPopup.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using RRReddit.Models;
using RRReddit.Data;
using System.Xml.Linq;
using MongoDB.Bson;

public class PostController : Controller
{
   // private static List<Post> _posts = new List<Post>();
    private readonly IMongoCollection<Post>? _postsCollection;
    private readonly IMongoCollection<Comment>? _comments;

    private readonly IMongoCollection<DatabaseUser>? _users;

    public PostController(MongoDatabase mongoDatabase)
    {
        _postsCollection = mongoDatabase.Database?.GetCollection<Post>("posts");
        _comments = mongoDatabase.Database?.GetCollection<Comment>("comments");

        _users = mongoDatabase.Database?.GetCollection<DatabaseUser>("users");
    }

    // GET: Show the form to create a new post
    [HttpGet]
    public IActionResult Create(string subredditName)
    {
        var post = new Post
        {
            SubredditName = subredditName
        };
        return View("CreatePost", post);
    }
    // POST: Save the new post
    [HttpPost]
    public async Task<IActionResult> Create(Post post)
    {
        var email = HttpContext.Session.GetString("UserEmail");
        var name = email.Split('@')[0];

        // Set the post properties before validation
        post.CreatedAt = DateTime.Now;
        post.User = name;

        // Ensure SubredditName is not null
        if (string.IsNullOrEmpty(post.SubredditName))
        {
            ModelState.AddModelError("SubredditName", "Subreddit name is required.");
        }

        if (!ModelState.IsValid)
        {
            return View("CreatePost", post);
        }

        // Add post to the database
        await _postsCollection.InsertOneAsync(post);

        // Redirect back to the subreddit page
        return RedirectToAction(post.SubredditName, "Home");
    }

    // GET: Return posts in subreddit
    public async Task<IActionResult> SubPosts(string subRedditName)
{
    var filter = Builders<Post>.Filter.Eq(post => post.SubredditName, subRedditName);
    var posts = await _postsCollection.Find(filter).ToListAsync();

    return View("SubPosts", posts);
}


    // GET: Show list of all posts
    public async Task<IActionResult> Index()
    {
        var email = HttpContext.Session.GetString("UserEmail");

        if (!string.IsNullOrEmpty(email))
        {
            var name = email.Split('@')[0];
            ViewData["AccountName"] = name;
        }

        var posts = await _postsCollection.Find(new BsonDocument()).ToListAsync();

        return View("IndexPost", posts);
    }


    // GET: Show details of a specific post
    [HttpGet("Post/Details/{postId}")]
    public async Task<IActionResult> Details(string postId)
    {
        var email = HttpContext.Session.GetString("UserEmail");

        if (!string.IsNullOrEmpty(email))
        {
            var name = email.Split('@')[0];
            ViewData["AccountName"] = name;
        }

        var filter = Builders<Post>.Filter.Eq(p => p.PostId, postId);
        var post = await _postsCollection.Find(filter).FirstOrDefaultAsync();
        if (post == null)
        {
            return NotFound();
        }

        var comments = await _comments.Find(c => c.PostId == postId).ToListAsync();
        ViewData["Comments"] = comments;

        return View("DetailsPost", post);
    }


    // GET: Confirm deletion of a specific post
    public async Task<IActionResult> Delete(string postId)
    {
        var filter = Builders<Post>.Filter.Eq(p => p.PostId, postId);
        var post = await _postsCollection.Find(filter).FirstOrDefaultAsync();
        if (post == null)
        {
            return NotFound();
        }

        return View("DeletePost", post);
    }

    // POST: Delete a specific post
    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(string postId)
    {
        var filter = Builders<Post>.Filter.Eq(p => p.PostId, postId);
        var post = await _postsCollection.Find(filter).FirstOrDefaultAsync();
        if (post != null)
        {
            await _postsCollection.DeleteOneAsync(filter);
            return RedirectToAction(post.SubredditName, "Home");
        }

        return NotFound();
    }


    //// upvoting and downvoting stuff

    [HttpPost("/Post/UpdateScore")]
    public async Task<IActionResult> UpdateScore(string postId, string voteType)
    {
        if (voteType != "up" && voteType != "down")
        {
            return Json(new { error = "Invalid vote type." });
        }

        // Convert the postId string to an ObjectId
        if (!ObjectId.TryParse(postId, out ObjectId objectId))
        {
            return Json(new { error = "Invalid post ID format." });
        }

        // Define the filter for the post
        var postFilter = Builders<Post>.Filter.Eq("PostId", objectId);

        // Retrieve user email and extract the username
        var email = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(email))
        {
            return Json(new { error = "User not authenticated." });
        }
        var name = email.Split('@')[0];

        // Query the user by username in the database
        var user = await _users.Find(u => u.UserName == name).FirstOrDefaultAsync();
        if (user == null)
        {
            return Json(new { error = "User not found." });
        }

        // Check if the post is already in the user's upvoted and downvoted arrays
        bool isUpvoted = user.Upvotes.Contains(objectId);
        bool isDownvoted = user.Downvotes.Contains(objectId);

        // Initialize score change
        int scoreChange = 0;

        if (voteType == "up")
        {
            if (!isUpvoted)
            {
                scoreChange = 1;
                var addToUpvotes = Builders<DatabaseUser>.Update.Push(u => u.Upvotes, objectId);
                await _users.UpdateOneAsync(u => u.UserName == name, addToUpvotes);

                if (isDownvoted)
                {
                    var removeFromDownvotes = Builders<DatabaseUser>.Update.Pull(u => u.Downvotes, objectId);
                    await _users.UpdateOneAsync(u => u.UserName == name, removeFromDownvotes);
                    scoreChange += 1; // Adjust score if removing downvote
                }
            }
            else
            {
                scoreChange = -1;
                var removeFromUpvotes = Builders<DatabaseUser>.Update.Pull(u => u.Upvotes, objectId);
                await _users.UpdateOneAsync(u => u.UserName == name, removeFromUpvotes);
            }
        }
        else if (voteType == "down")
        {
            if (!isDownvoted)
            {
                scoreChange = -1;
                var addToDownvotes = Builders<DatabaseUser>.Update.Push(u => u.Downvotes, objectId);
                await _users.UpdateOneAsync(u => u.UserName == name, addToDownvotes);

                if (isUpvoted)
                {
                    var removeFromUpvotes = Builders<DatabaseUser>.Update.Pull(u => u.Upvotes, objectId);
                    await _users.UpdateOneAsync(u => u.UserName == name, removeFromUpvotes);
                    scoreChange -= 1; // Adjust score if removing upvote
                }
            }
            else
            {
                scoreChange = 1;
                var removeFromDownvotes = Builders<DatabaseUser>.Update.Pull(u => u.Downvotes, objectId);
                await _users.UpdateOneAsync(u => u.UserName == name, removeFromDownvotes);
            }
        }

        // Update the score on the post
        var scoreUpdate = Builders<Post>.Update.Inc(p => p.Score, scoreChange);
        await _postsCollection.UpdateOneAsync(postFilter, scoreUpdate);

        // Retrieve the updated post score
        var updatedPost = await _postsCollection.Find(postFilter).FirstOrDefaultAsync();
        return Json(new { newScore = updatedPost.Score });
    }


    [HttpGet]
    public async Task<IActionResult> SearchBar(string query)
    {
        // Create a filter that checks for partial matches in MovieName, Title, or Content.
        var filter = Builders<Post>.Filter.Or(
            Builders<Post>.Filter.Regex(post => post.MovieName, new BsonRegularExpression(query, "i")),
            Builders<Post>.Filter.Regex(post => post.Title, new BsonRegularExpression(query, "i")),
            Builders<Post>.Filter.Regex(post => post.Content, new BsonRegularExpression(query, "i"))
        );

        // Execute the query to find matching posts.
        var result = await _postsCollection.Find(filter).ToListAsync();
        ViewData["Query"] = query;

        var viewModel = new SubredditViewModel
        {
            SubredditName = query,
            Posts = result
        };

        // Set a message if no results are found.
        ViewData["ResultMessage"] = result.Count == 0 ? "No Results Found." : "";

        return View("SearchResult", viewModel);
    }





}