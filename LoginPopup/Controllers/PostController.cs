using LoginPopup.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using RRReddit.Models;
using RRReddit.Data;

public class PostController : Controller
{
    private static List<Post> _posts = new List<Post>();
    private readonly IMongoCollection<Post>? _postsCollection;
    private readonly IMongoCollection<Comment>? _comments;

    public PostController(MongoDatabase mongoDatabase)
    {
        _postsCollection = mongoDatabase.Database?.GetCollection<Post>("posts");
        _comments = mongoDatabase.Database?.GetCollection<Comment>("comments");
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

}
