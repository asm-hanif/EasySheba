using EasySheba.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasySheba.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GlobalSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new { doctors = new List<object>(), tests = new List<object>(), beds = new List<object>() });
            }

            try
            {
                // Search Doctors
                var doctors = await _context.Doctors
                    .Where(d => d.Name.Contains(query) ||
                                d.Specialist.Contains(query) ||
                                d.Department.Contains(query) ||
                                d.HospitalName.Contains(query))
                    .Select(d => new { d.Name, d.Specialist })
                    .Take(5)
                    .ToListAsync();

                // Search Medical Tests
                var tests = await _context.MedicalTests
                    .Where(t => t.TestName.Contains(query) ||
                                t.HospitalName.Contains(query))
                    .Select(t => new { t.TestName, t.HospitalName })
                    .Take(5)
                    .ToListAsync();

                // Search Beds
                var beds = await _context.Beds
                    .Where(b => b.BedType.Contains(query) ||
                                b.HospitalName.Contains(query))
                    .Select(b => new { b.BedType, b.HospitalName })
                    .Take(5)
                    .ToListAsync();

                return Json(new { doctors, tests, beds });
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Search error: {ex.Message}");
                return Json(new { doctors = new List<object>(), tests = new List<object>(), beds = new List<object>() });
            }
        }
    }
}