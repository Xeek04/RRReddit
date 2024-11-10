using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using RRReddit.Models;
using System.Diagnostics;
using RRReddit.Data;

namespace RRReddit.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        
        private readonly IMongoCollection<DatabaseUser>? _users;
        private readonly IMongoCollection<DatabaseUser>? _subreddits;
        private readonly IMongoCollection<Post>? _posts; 

        public HomeController(ILogger<HomeController> logger, MongoDatabase mongoDatabase)
        {
            _logger = logger;

            _users = mongoDatabase.Database?.GetCollection<DatabaseUser>("users");
            _subreddits = mongoDatabase.Database?.GetCollection<DatabaseUser>("subreddits");
            _posts = mongoDatabase.Database?.GetCollection<Post>("posts");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Explore()
        {
            return View();
        }

        public IActionResult Info()
        {
            return View();
        }

        public IActionResult Subreddit()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }



        [HttpGet]
        public async Task<IActionResult> GetUsername()
        {
            /*
            var test = await _users.Find(_ => true).ToListAsync();
            Console.WriteLine(test[0].UserName);
            return View("Index");
            */

            var users = await _users.Find(_ => true).ToListAsync();
            if (users.Count > 0)
            {
                ViewData["Username"] = users[0].UserName; // Storing the username in ViewData
            }
            else
            {
                ViewData["Username"] = "No users found"; // Fallback if no user is found
            }

            return View("Index");


        }

        [HttpPost]
        public async Task<IActionResult> UpdateUsername(string user, string newValue)
        {
            var search = Builders<DatabaseUser>.Filter.Eq(DatabaseUser => DatabaseUser.UserName, user); // affected user
            var update = Builders<DatabaseUser>.Update.Set(DatabaseUser => DatabaseUser.UserName, newValue); // new value
            await _users.UpdateOneAsync(search, update);
            return View("Index");
        }



        /* this function is used in just showing the subreddits a user has in the database. called in account page for "VIEW SUBREDDITS" button */
        [HttpGet]
        public async Task<IActionResult> GetSubreddits()
        {
            //fetching the user's subreddits field as a single string from the database
            var user = await _users.Find(_ => true).FirstOrDefaultAsync();
            var subredditsString = user.Subreddits;

            //if string is empty/null then give a defulat message
            if (string.IsNullOrEmpty(subredditsString))
            {
                subredditsString = "Your not apart of any yet... Join some Subreddits!!";
            }

            // Split the string by comma to get an array
            var subredditList = subredditsString.Split(", ").ToList();

            // Return the list as JSON
            return Json(subredditList);
        }



        /* this function is used in modyfing the subreddits during actions of joining/unjoing. called in each subreddit pages */
        [HttpGet]
        public async Task<IActionResult> GetJoinedSubreddits()
        {
            var user = await _users.Find(_ => true).FirstOrDefaultAsync(); 
            var subredditsString = user?.Subreddits ?? "";

            return Json(subredditsString.Split(", ").ToList());
        }



        [HttpPost]
        public async Task<IActionResult> UpdateSubredditStatus([FromBody] UpdateSubredditRequest request)
        {
            //logs for debugging. these will appear in server console not view console. they get called with joined button in subreddits pages
            Console.WriteLine($"Received request to update subreddits: {request.UpdatedSubreddits}");

            //fetching user
            var user = await _users.Find(_ => true).FirstOrDefaultAsync();
            if (user == null)
            {
                Console.WriteLine("User not found");
                return BadRequest(new { success = false, message = "User not found" });
            }

            var currentSubredditsString = user.Subreddits ?? "Your not apart of any yet... Join some Subreddits!!";
            Console.WriteLine($"Current subreddits in database: {currentSubredditsString}");

            // If the current string is the default message, initialize the list as empty
            var currentSubredditsList = currentSubredditsString == "Your not apart of any yet... Join some Subreddits!!"
                                        ? new List<string>()
                                        : currentSubredditsString.Split(", ").ToList();

            // Update the list based on the incoming request
            var updatedSubredditsList = request.UpdatedSubreddits.Split(", ").Where(s => !string.IsNullOrEmpty(s)).ToList();
            Console.WriteLine($"Updated subreddits list to save: {string.Join(", ", updatedSubredditsList)}");

            // If the lists are the same, there's nothing to update
            if (currentSubredditsList.SequenceEqual(updatedSubredditsList))
            {
                Console.WriteLine("No changes detected, skipping database update");
                return Json(new { success = true, message = "No changes made" });
            }

            // Update the user's subreddits field in the database
            var updateFilter = Builders<DatabaseUser>.Filter.Eq(u => u.Id, user.Id);
            var updateDefinition = Builders<DatabaseUser>.Update.Set(u => u.Subreddits, string.Join(", ", updatedSubredditsList));

            var updateResult = await _users.UpdateOneAsync(updateFilter, updateDefinition);
            if (updateResult.ModifiedCount > 0)
            {
                Console.WriteLine($"Successfully updated subreddits to: {string.Join(", ", updatedSubredditsList)}");
                return Json(new { success = true, message = "Subreddits updated successfully" });
            }
            else
            {
                Console.WriteLine("Database update failed");
                return BadRequest(new { success = false, message = "Database update failed" });
            }
        }
        //added class in UserModel to handle incoming request





        /* each subreddit gets called in layout and explore */
        public IActionResult Action()
        {
            return View();
        }

        public IActionResult Comedy()
        {
            return View();
        }

        public IActionResult Drama()
        {
            return View();
        }

        public IActionResult Fantasy()
        {
            return View();
        }

        public IActionResult Horror()
        {
            return View();
        }

        public IActionResult Mystery()
        {
            return View();
        }

        public IActionResult Romance()
        {
            return View();
        }

        public IActionResult ScienceFiction()
        {
            return View();
        }

        public IActionResult Thriller()
        {
            return View();
        }


        
        /* here is where messing with array for upvotes and downvotes */

        [HttpGet]
        public async Task<IActionResult> GetUserUpvotes()
        {
            //get user email and then split it to only get name before @
            var email = HttpContext.Session.GetString("UserEmail");
            var name = email.Split('@')[0];

            //Query the user by name which is username in database
            var user = await _users.Find(u => u.UserName == name).FirstOrDefaultAsync();

            if (user != null && user.Upvotes != null && user.Upvotes.Any())
            {
                // Fetch the posts corresponding to the ObjectIds in the Upvotes array
                var filter = Builders<Post>.Filter.In(post => post.PostId, user.Upvotes);
                var upvotedPosts = await _posts.Find(filter).ToListAsync();      

                //grab the title and content for each post 
                var result = upvotedPosts.Select(post => new
                {
                    PostId = post.PostId.ToString(),
                    Title = post.Title,
                    Content = post.Content
                }).ToList();

                return Json(result); //need as JSON to work with
            }
            else
            {
                return Json(new List<object>()); //if-else statement handles if no upvotesfound: return an empty list 
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetUserDownvotes()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var name = email.Split('@')[0];

            var user = await _users.Find(u => u.UserName == name).FirstOrDefaultAsync();

            if (user != null && user.Downvotes != null && user.Downvotes.Any())
            {

                var filter = Builders<Post>.Filter.In(post => post.PostId, user.Downvotes);
                var downvotedPosts = await _posts.Find(filter).ToListAsync();

                var result = downvotedPosts.Select(post => new
                {
                    PostId = post.PostId.ToString(),
                    Title = post.Title,
                    Content = post.Content
                }).ToList();

                return Json(result); 
            }
            else
            {
                return Json(new List<object>());
            }
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
