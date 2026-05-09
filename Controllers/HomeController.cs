using System.Diagnostics;
using EasySheba.Data;
using EasySheba.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EasySheba.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
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

            // Fetch real counts
            var totalDoctors = await _context.Doctors.CountAsync();
            var approvedHospitals = await _context.HospitalAdminProfiles.CountAsync(h => h.IsApproved);
            var happyPatients = await _context.Appointments.Select(a => a.PatientId).Distinct().CountAsync();
            if (happyPatients == 0) happyPatients = 5000; // fallback

            ViewBag.DoctorCount = totalDoctors;
            ViewBag.HospitalCount = approvedHospitals;
            ViewBag.PatientCount = happyPatients;

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