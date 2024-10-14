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
            _firebaseAuth = new FirebaseAuthProvider(new FirebaseConfig("Your_Firebase_API_Key"));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AccessToken");
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
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
                return View("create", vm);
            }

            try
            {
                // Create the user with Firebase Authentication
                await _firebaseAuth.CreateUserWithEmailAndPasswordAsync(vm.EmailAddress, vm.Password);

                // Sign in the user to get the token
                var firebaselink = await _firebaseAuth.SignInWithEmailAndPasswordAsync(vm.EmailAddress, vm.Password);
                string accessToken = firebaselink.FirebaseToken;

                if (accessToken != null)
                {
                    HttpContext.Session.SetString("AccessToken", accessToken);
                    // Redirect to Index or show a success message
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Authentication failed.");
                    return View("create", vm);
                }
            }
            catch (FirebaseAuthException ex)
            {
                // Handle Firebase authentication exceptions
                var errorMessage = ex.Reason.ToString();
                ModelState.AddModelError(string.Empty, errorMessage);
                return View("create", vm);
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

        // Other actions...
    }
}
