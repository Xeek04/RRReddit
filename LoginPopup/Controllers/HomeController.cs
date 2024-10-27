using Firebase.Auth;
using LoginPopup.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace LoginPopup.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FirebaseAuthProvider _firebaseAuth;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _firebaseAuth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyCacz_YoziQxAIwvZExl2hP3hwXm8IdMWs"));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AccessToken");
<<<<<<< HEAD
            return RedirectToAction("Info");
=======
            return RedirectToAction("Index");
>>>>>>> master
        }

        public IActionResult Index()
        {
            return View();
        }

<<<<<<< HEAD
        [HttpGet]
=======
>>>>>>> master
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
<<<<<<< HEAD
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


                string accessToken = result.FirebaseToken;

                if (!string.IsNullOrEmpty(accessToken))
                {
                    HttpContext.Session.SetString("AccessToken", accessToken);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Authentication failed. Invalid token.");
                    return View(vm);
                }
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
=======
        [HttpPost]
>>>>>>> master

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
<<<<<<< HEAD
                return View("LoginPartial", vm);
=======
                return View("create", vm);
>>>>>>> master
            }

            try
            {
                // Sign in the user to get the token
                var firebaselink = await _firebaseAuth.SignInWithEmailAndPasswordAsync(vm.EmailAddress, vm.Password);
                string accessToken = firebaselink.FirebaseToken;

                if (!string.IsNullOrEmpty(accessToken))
                {
<<<<<<< HEAD
                    //storing access token and email in session
                    HttpContext.Session.SetString("AccessToken", accessToken);
                    HttpContext.Session.SetString("UserEmail", vm.EmailAddress);
=======
                    HttpContext.Session.SetString("AccessToken", accessToken);
>>>>>>> master
                    // Redirect to Index or show a success message
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Authentication failed. Invalid token.");
<<<<<<< HEAD
                    return View("LoginPartial", vm);
=======
                    return View("create", vm);
>>>>>>> master
                }
            }
            catch (FirebaseAuthException ex)
            {
                // Handle Firebase authentication exceptions
                var errorMessage = GetFirebaseAuthErrorMessage(ex);
                ModelState.AddModelError(string.Empty, errorMessage);
<<<<<<< HEAD
                return View("LoginPartial", vm);
=======
                return View("create", vm);
>>>>>>> master
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
<<<<<<< HEAD
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


=======
            return View();
        }

>>>>>>> master
        public IActionResult Testpage()
        {
            return View();
        }
<<<<<<< HEAD
=======
        // Other actions...
>>>>>>> master
    }
}
