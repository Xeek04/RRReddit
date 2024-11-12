using Firebase.Auth;
using LoginPopup.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RRReddit.Models;
using RRReddit.Data;
using MongoDB.Bson;

namespace LoginPopup.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IMongoCollection<DatabaseUser>? _users;
        private readonly IMongoCollection<DatabaseUser>? _subreddits;
        private readonly IMongoCollection<Post>? _posts; 



        private readonly FirebaseAuthProvider _firebaseAuth;

        public HomeController(ILogger<HomeController> logger, MongoDatabase mongoDatabase)
        {
            _logger = logger;

            _firebaseAuth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyCacz_YoziQxAIwvZExl2hP3hwXm8IdMWs"));

            _users = mongoDatabase.Database?.GetCollection<DatabaseUser>("users");
            _subreddits = mongoDatabase.Database?.GetCollection<DatabaseUser>("subreddits");
            _posts = mongoDatabase.Database?.GetCollection<Post>("posts");
        }
  

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AccessToken");
            return RedirectToAction("Info");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(LoginModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            try
            {
                var result = await _firebaseAuth.CreateUserWithEmailAndPasswordAsync(vm.EmailAddress, vm.Password);
                await _firebaseAuth.SendEmailVerificationAsync(result.FirebaseToken);

                // Do not sign in the user automatically
                // Redirect to a "Check Your Email" page or display a success message
                return RedirectToAction("CheckYourEmail");
            }
            catch (FirebaseAuthException ex)
            {
                // Handle Firebase authentication exceptions
                var errorMessage = GetFirebaseAuthErrorMessage(ex);
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(vm);
            }
        }


        [HttpPost]
        //[HttpPost]

        private string GetFirebaseAuthErrorMessage(FirebaseAuthException ex)
        {
            switch (ex.Reason)
            {
                case AuthErrorReason.WrongPassword:
                    return "Incorrect password. Please try again.";
                case AuthErrorReason.UnknownEmailAddress:
                    return "Email address not found. Please check and try again.";
                case AuthErrorReason.InvalidEmailAddress:
                    return "Invalid email address format.";
                case AuthErrorReason.UserDisabled:
                    return "This user account has been disabled.";
                // Add more cases as needed for different error reasons
                default:
                    return "An error occurred during authentication. Please try again.";
            }
        }

        public async Task<IActionResult> LoginUser(LoginModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Log validation errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError(error.ErrorMessage);
                }
                // Return the view with validation errors
                return View("LoginPartial", vm);
            }

            try
            {
                // Sign in the user to get the token
                var firebaseLink = await _firebaseAuth.SignInWithEmailAndPasswordAsync(vm.EmailAddress, vm.Password);
                var user = await _firebaseAuth.GetUserAsync(firebaseLink.FirebaseToken);

                if (!user.IsEmailVerified)
                {
                    ModelState.AddModelError(string.Empty, "Please verify your email before logging in.");
                    return View("LoginPartial", vm);
                }

                string accessToken = firebaseLink.FirebaseToken;

                if (!string.IsNullOrEmpty(accessToken))
                {
                    // Store access token and email in session
                    HttpContext.Session.SetString("AccessToken", accessToken);
                    HttpContext.Session.SetString("UserEmail", vm.EmailAddress);
                    // Redirect to Index or show a success message
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Authentication failed. Invalid token.");
                    return View("LoginPartial", vm);
                }
            }
            catch (FirebaseAuthException ex)
            {
                // Handle Firebase authentication exceptions
                var errorMessage = GetFirebaseAuthErrorMessage(ex);
                ModelState.AddModelError(string.Empty, errorMessage);
                return View("LoginPartial", vm);
            }
        }



        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Button()
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccountPage()
        {
            // Get the email from the session
            var email = HttpContext.Session.GetString("UserEmail");

            if (!string.IsNullOrEmpty(email))
            {
                // Extract the name from the email (everything before '@')
                var name = email.Split('@')[0];

                ViewData["AccountName"] = name;
            }

            return View();
        }


        public IActionResult Testpage()
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
            //get user email and then split it to only get name before @
            var email = HttpContext.Session.GetString("UserEmail");
            var name = email.Split('@')[0];

            //checking to make sure user logged in
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { message = "User not logged in." });
            }

            //Query the user by name which is username in database
            var user = await _users.Find(u => u.UserName == name).FirstOrDefaultAsync();

            //checking if user was found
            if (user == null)
            {
                return Json(new { message = "User not found." });
            }

            //get string
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
            //get user email and then split it to only get name before @
            var email = HttpContext.Session.GetString("UserEmail");
            var name = email.Split('@')[0];

            //checking to make sure user logged in
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { message = "User not logged in." });
            }

            //Query the user by name which is username in database
            var user = await _users.Find(u => u.UserName == name).FirstOrDefaultAsync();

            //checking if user was found
            if (user == null)
            {
                return Json(new { message = "User not found." });
            }

            var subredditsString = user?.Subreddits ?? "";

            return Json(subredditsString.Split(", ").ToList());
        }



        [HttpPost]
        public async Task<IActionResult> UpdateSubredditStatus([FromBody] UpdateSubredditRequest request)
        {
            //logs for debugging. these will appear in server console not view console. they get called with joined button in subreddits pages
            Console.WriteLine($"Received request to update subreddits: {request.UpdatedSubreddits}");

            //get user email and then split it to only get name before @
            var email = HttpContext.Session.GetString("UserEmail");
            var name = email.Split('@')[0];

            //checking to make sure user logged in
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { message = "User not logged in." });
            }

            //Query the user by name which is username in database
            var user = await _users.Find(u => u.UserName == name).FirstOrDefaultAsync();

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
            string subredditName = "Action";

            // Filter posts belonging to this subreddit
            var subredditPosts = DataStore.Posts.Where(p => p.SubredditName == subredditName).ToList();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Action", viewModel);
        }

        public IActionResult Comedy()
        {
            string subredditName = "Comedy";

            // Filter posts belonging to this subreddit
            var subredditPosts = DataStore.Posts.Where(p => p.SubredditName == subredditName).ToList();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Comedy", viewModel);
        }

        public IActionResult Drama()
        {
            string subredditName = "Drama";

            // Filter posts belonging to this subreddit
            var subredditPosts = DataStore.Posts.Where(p => p.SubredditName == subredditName).ToList();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Drama", viewModel);
        }

        public IActionResult Fantasy()
        {
            string subredditName = "Fantasy";

            // Filter posts belonging to this subreddit
            var subredditPosts = DataStore.Posts.Where(p => p.SubredditName == subredditName).ToList();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Fantasy", viewModel);
        }

        public IActionResult Horror()
        {
            string subredditName = "Horror";

            // Filter posts belonging to this subreddit
            var subredditPosts = DataStore.Posts.Where(p => p.SubredditName == subredditName).ToList();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Horror", viewModel);
        }

        public IActionResult Mystery()
        {
            string subredditName = "Mystery";

            // Filter posts belonging to this subreddit
            var subredditPosts = DataStore.Posts.Where(p => p.SubredditName == subredditName).ToList();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Mystery", viewModel);
        }

        public IActionResult Romance()
        {
            string subredditName = "Romance";

            // Filter posts belonging to this subreddit
            var subredditPosts = DataStore.Posts.Where(p => p.SubredditName == subredditName).ToList();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Romance", viewModel);
        }

        public IActionResult ScienceFiction()
        {
            string subredditName = "ScienceFiction";

            // Filter posts belonging to this subreddit
            var subredditPosts = DataStore.Posts.Where(p => p.SubredditName == subredditName).ToList();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("ScienceFiction", viewModel);
        }

        public IActionResult Thriller()
        {
            string subredditName = "Thriller";

            // Filter posts belonging to this subreddit
            var subredditPosts = DataStore.Posts.Where(p => p.SubredditName == subredditName).ToList();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Thriller", viewModel);
        }




        ///////////////////////////////* here is where messing with array for upvotes and downvotes */

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




        ////////////////////////////bookmarking stuff now

        [HttpPost("/Post/ToggleBookmark")]
        public async Task<IActionResult> ToggleBookmark(string postId)
        {
            // Convert the postId string to an ObjectId
            if (!ObjectId.TryParse(postId, out ObjectId objectId))
            {
                return Json(new { error = "Invalid post ID format." });
            }

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

            // Check if the post is already bookmarked by the user
            bool isBookmarked = user.Bookmarks.Contains(objectId);

            // Update the bookmarks array based on the current state
            if (!isBookmarked)
            {
                // Add post ID to the user's bookmarks array
                var addToBookmarks = Builders<DatabaseUser>.Update.Push(u => u.Bookmarks, objectId);
                await _users.UpdateOneAsync(u => u.UserName == name, addToBookmarks);
            }
            else
            {
                // Remove post ID from the user's bookmarks array
                var removeFromBookmarks = Builders<DatabaseUser>.Update.Pull(u => u.Bookmarks, objectId);
                await _users.UpdateOneAsync(u => u.UserName == name, removeFromBookmarks);
            }

            return Json(new { success = true });
        }




        [HttpGet]
        public async Task<IActionResult> GetUserBookmarks()
        {
            // Retrieve the user's email from the session and extract the username
            var email = HttpContext.Session.GetString("UserEmail");
            var name = email?.Split('@')[0];

            if (string.IsNullOrEmpty(name))
            {
                return Json(new { error = "User not authenticated." });
            }

            // Query the user by their username
            var user = await _users.Find(u => u.UserName == name).FirstOrDefaultAsync();

            if (user != null && user.Bookmarks != null && user.Bookmarks.Any())
            {
                // Fetch posts based on the ObjectIds in the bookmarks array
                var filter = Builders<Post>.Filter.In(post => post.PostId, user.Bookmarks);
                var bookmarkedPosts = await _posts.Find(filter).ToListAsync();

                // Prepare the data to include the tag for each bookmarked post
                var result = bookmarkedPosts.Select(post => new
                {
                    PostId = post.PostId.ToString(),
                    Tag = post.Tag
                }).ToList();

                return Json(result); // Return the bookmarked posts as JSON
            }
            else
            {
                return Json(new List<object>()); // Return an empty list if no bookmarks found
            }
        }








        public IActionResult CheckYourEmail()
        {
            return View();
        }

    }
}
