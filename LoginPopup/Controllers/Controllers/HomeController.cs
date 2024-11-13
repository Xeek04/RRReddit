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
        private readonly IMongoCollection<Post> _postsCollection;


        private readonly FirebaseAuthProvider _firebaseAuth;

        public HomeController(ILogger<HomeController> logger, MongoDatabase mongoDatabase)
        {
            _logger = logger;

            _firebaseAuth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyCacz_YoziQxAIwvZExl2hP3hwXm8IdMWs"));

            _users = mongoDatabase.Database?.GetCollection<DatabaseUser>("users");
            _subreddits = mongoDatabase.Database?.GetCollection<DatabaseUser>("subreddits");
            _postsCollection = mongoDatabase.Database?.GetCollection<Post>("posts");
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AccessToken");
            return RedirectToAction("Info");
        }

        public async Task<IActionResult> Index()
        {
            // List of subreddits you want to show posts from
            var subreddits = new List<string> { "Action", "Comedy", "Drama", "Fantasy" };

            // List to hold the selected posts
            var selectedPosts = new List<Post>();

            foreach (var subreddit in subreddits)
            {
                // Get the most recent post from the subreddit (sorted by CreatedAt descending)
                var filter = Builders<Post>.Filter.Eq(post => post.SubredditName, subreddit);
                var post = await _postsCollection
                    .Find(filter)
                    .SortByDescending(post => post.CreatedAt)
                    .FirstOrDefaultAsync();

                if (post != null)
                {
                    selectedPosts.Add(post);
                }
            }

            // Pass the selected posts to the view
            return View(selectedPosts);
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

        public IActionResult SearchResult()
        {
            return View();
        }


        /*public IActionResult Subreddit()
        {
            return View();
        }*/

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
        public async Task<IActionResult> Action()
        {
            string subredditName = "Action";

            // Fetch posts from the database where SubredditName matches
            var filter = Builders<Post>.Filter.Eq(p => p.SubredditName, subredditName);
            var subredditPosts = await _postsCollection.Find(filter).ToListAsync();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Action", viewModel);
        }


        public async Task<IActionResult> Comedy()
        {
            string subredditName = "Comedy";

            var filter = Builders<Post>.Filter.Eq(p => p.SubredditName, subredditName);
            var subredditPosts = await _postsCollection.Find(filter).ToListAsync();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Comedy", viewModel);
        }


        public async Task<IActionResult> Drama()
        {
            string subredditName = "Drama";

            var filter = Builders<Post>.Filter.Eq(p => p.SubredditName, subredditName);
            var subredditPosts = await _postsCollection.Find(filter).ToListAsync();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Drama", viewModel);
        }

        public async Task<IActionResult> Fantasy()
        {
            string subredditName = "Fantasy";

            var filter = Builders<Post>.Filter.Eq(p => p.SubredditName, subredditName);
            var subredditPosts = await _postsCollection.Find(filter).ToListAsync();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Fantasy", viewModel);
        }

        public async Task<IActionResult> Horror()
        {
            string subredditName = "Horror";

            var filter = Builders<Post>.Filter.Eq(p => p.SubredditName, subredditName);
            var subredditPosts = await _postsCollection.Find(filter).ToListAsync();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Horror", viewModel);
        }

        public async Task<IActionResult> Mystery()
        {
            string subredditName = "Mystery";

            var filter = Builders<Post>.Filter.Eq(p => p.SubredditName, subredditName);
            var subredditPosts = await _postsCollection.Find(filter).ToListAsync();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Mystery", viewModel);
        }

        public async Task<IActionResult> Romance()
        {
            string subredditName = "Romance";

            var filter = Builders<Post>.Filter.Eq(p => p.SubredditName, subredditName);
            var subredditPosts = await _postsCollection.Find(filter).ToListAsync();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Romance", viewModel);
        }

        public async Task<IActionResult> ScienceFiction()
        {
            string subredditName = "ScienceFiction";

            var filter = Builders<Post>.Filter.Eq(p => p.SubredditName, subredditName);
            var subredditPosts = await _postsCollection.Find(filter).ToListAsync();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("ScienceFiction", viewModel);
        }

        public async Task<IActionResult> Thriller()
        {
            string subredditName = "Thriller";

            var filter = Builders<Post>.Filter.Eq(p => p.SubredditName, subredditName);
            var subredditPosts = await _postsCollection.Find(filter).ToListAsync();

            var viewModel = new SubredditViewModel
            {
                SubredditName = subredditName,
                Posts = subredditPosts
            };

            return View("Thriller", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserUpvotes()
        {
            // Get user email and extract the username
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { error = "User not authenticated." });
            }
            var name = email.Split('@')[0];

            // Query the user by name which is username in database
            var user = await _users.Find(u => u.UserName == name).FirstOrDefaultAsync();

            if (user != null && user.Upvotes != null && user.Upvotes.Any())
            {
                // Convert ObjectId to string
                var upvoteIds = user.Upvotes.Select(id => id.ToString()).ToList();

                // Create filter using string PostId
                var filter = Builders<Post>.Filter.In(post => post.PostId, upvoteIds);
                // Alternatively, using field name as string
                // var filter = Builders<Post>.Filter.In("PostId", upvoteIds);

                var upvotedPosts = await _postsCollection.Find(filter).ToListAsync();

                // Prepare the result
                var result = upvotedPosts.Select(post => new
                {
                    PostId = post.PostId,
                    Title = post.Title,
                    Content = post.Content
                }).ToList();

                return Json(result); // Return as JSON for frontend consumption
            }
            else
            {
                return Json(new List<object>()); // Return an empty list if no upvotes found
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetUserDownvotes()
        {
            // Retrieve the user's email from the session
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { error = "User not authenticated." });
            }

            // Extract the username from the email
            var name = email.Split('@')[0];

            // Query the user by username in the database
            var user = await _users.Find(u => u.UserName == name).FirstOrDefaultAsync();

            if (user != null && user.Downvotes != null && user.Downvotes.Any())
            {
                // Convert ObjectIds to string representations
                var downvoteIds = user.Downvotes.Select(id => id.ToString()).ToList();

                // Create a filter using the string PostId
                var filter = Builders<Post>.Filter.In(post => post.PostId, downvoteIds);
                // Alternatively, using field name as string:
                // var filter = Builders<Post>.Filter.In("PostId", downvoteIds);

                // Fetch the downvoted posts from the database
                var downvotedPosts = await _postsCollection.Find(filter).ToListAsync();

                // Prepare the result to be returned as JSON
                var result = downvotedPosts.Select(post => new
                {
                    PostId = post.PostId,
                    Title = post.Title,
                    Content = post.Content
                }).ToList();

                return Json(result);
            }
            else
            {
                // Return an empty list if no downvotes are found
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
            // Retrieve the user's email from the session
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { error = "User not authenticated." });
            }

            // Extract the username from the email
            var name = email.Split('@')[0];

            // Query the user by username in the database
            var user = await _users.Find(u => u.UserName == name).FirstOrDefaultAsync();

            if (user != null && user.Bookmarks != null && user.Bookmarks.Any())
            {
                // Convert ObjectIds to string representations
                var bookmarkIds = user.Bookmarks.Select(id => id.ToString()).ToList();

                // Create a filter using the string PostId
                var filter = Builders<Post>.Filter.In(post => post.PostId, bookmarkIds);
                // Alternatively, using field name as string:
                // var filter = Builders<Post>.Filter.In("PostId", bookmarkIds);

                // Fetch the bookmarked posts from the database
                var bookmarkedPosts = await _postsCollection.Find(filter).ToListAsync();

                // Prepare the result to be returned as JSON
                var result = bookmarkedPosts.Select(post => new
                {
                    PostId = post.PostId,
                    MovieName = post.MovieName
                }).ToList();

                return Json(result); // Return the bookmarked posts as JSON
            }
            else
            {
                // Return an empty list if no bookmarks are found
                return Json(new List<object>());
            }
        }

    }
}