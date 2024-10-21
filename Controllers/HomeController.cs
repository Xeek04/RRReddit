using Microsoft.AspNetCore.Mvc;
using RRReddit.Models;
using System.Diagnostics;

namespace RRReddit.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
