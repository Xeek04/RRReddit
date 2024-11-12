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
    private static List<Post> _posts = new List<Post>();
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
        if (!ModelState.IsValid)
        {
            return View("CreatePost", post);
        }

        var email = HttpContext.Session.GetString("UserEmail");
        var name = email.Split('@')[0];

        // Adding the post to the list of posts
        post.Id = DataStore.Posts.Count > 0 ? DataStore.Posts.Max(p => p.Id) + 1 : 1;
        post.Id = _posts.Count > 0 ? _posts.Max(p => p.Id) + 1 : 1;
        post.CreatedAt = DateTime.Now;
        post.User = name;
        _posts.Add(post);
        DataStore.Posts.Add(post);

        // Add post to database
        await _postsCollection.InsertOneAsync(post);

        // Redirect back to the subreddit page
        return RedirectToAction(post.SubredditName, "Home");
    }

    // GET: Return posts in subreddit
    public async Task<IActionResult> SubPosts(string subRedditName)
    {
        var filter = Builders<Post>.Filter.Eq(Post => Post.SubredditName, subRedditName);

        var result = await _postsCollection.Find(filter).ToListAsync();

        Console.WriteLine(result);

        return RedirectToAction(subRedditName, "Home");
    }

    // GET: Show list of all posts
    public IActionResult Index()
    {
        // Get the email from the session
        var email = HttpContext.Session.GetString("UserEmail");

        if (!string.IsNullOrEmpty(email))
        {
            // Extract the name from the email (everything before '@')
            var name = email.Split('@')[0];

            ViewData["AccountName"] = name;
        }

        return View("IndexPost", _posts);
    }

    // GET: Show details of a specific post
    public IActionResult Details(int id)
    {
        var email = HttpContext.Session.GetString("UserEmail");

        if (!string.IsNullOrEmpty(email))
        {
            // Extract the name from the email (everything before '@')
            var name = email.Split('@')[0];

            ViewData["AccountName"] = name;
        }
        var post = _posts.FirstOrDefault(p => p.Id == id);
        if (post == null)
        {
            return NotFound();
        }

        return View("DetailsPost", post);
    }

    // GET: Confirm deletion of a specific post
    public IActionResult Delete(int id)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);
        if (post == null)
        {
            return NotFound();
        }

        return View("DeletePost", post);
    }

    // POST: Delete a specific post
    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);
        if (post != null)
        {
            _posts.Remove(post);
        }

        return RedirectToAction(post.SubredditName, "Home");
    }





    /////////////////////////////////////////////////////// upvoting and downvoting stuff

    [HttpPost("/Post/UpdateScore")]
    public async Task<IActionResult> UpdateScore(string postId, string voteType)
    {
        //these are only two strings that should be getting passed
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

        //get user email and extract the username
        var email = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(email))
        {
            return Json(new { error = "User not authenticated." });
        }
        var name = email.Split('@')[0];

        // Query the user by name (username) in the database
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






}
