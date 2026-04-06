using System.Diagnostics;
using EasySheba.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EasySheba.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // ========== HOME PAGE ==========
        public async Task<IActionResult> Index()
        {
            // Redirect based on user role if logged in
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("HospitalAdmin"))
                {
                    return RedirectToAction("Index", "HospitalAdmin");
                }
                if (User.IsInRole("SuperAdmin"))
                {
                    return RedirectToAction("Index", "SuperAdmin");
                }
                // Patient stays on Home page
            }
            return View();
        }

        // ========== ABOUT PAGE ==========
        public IActionResult About()
        {
            return View();
        }

        // ========== PRIVACY PAGE ==========
        public IActionResult Privacy()
        {
            return View();
        }

        // ========== ERROR PAGE ==========
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}