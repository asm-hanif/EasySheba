using EasySheba.Data;
using EasySheba.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace EasySheba.Controllers
{
    public class MedicalTestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicalTestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tests = await _context.MedicalTests.ToListAsync();
            return View(tests);
        }

        // ============================
        // EDIT MEDICAL TEST (GET)
        // ============================
        [HttpGet]
        public async Task<IActionResult> EditMedicalTest(int id)
        {
            var test = await _context.MedicalTests.FindAsync(id);
            if (test == null) return NotFound();

            var selectedDays = string.IsNullOrEmpty(test.AvailableDays)
                ? new System.Collections.Generic.List<string>()
                : test.AvailableDays.Split(", ").ToList();
            ViewBag.SelectedDays = selectedDays;

            return View(test);
        }

        // ============================
        // EDIT MEDICAL TEST (POST)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicalTest(MedicalTest model, List<string> dayList, bool? removeImage, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                var selectedDays = string.IsNullOrEmpty(model.AvailableDays)
                    ? new System.Collections.Generic.List<string>()
                    : model.AvailableDays.Split(", ").ToList();
                ViewBag.SelectedDays = selectedDays;
                return View(model);
            }

            var existingTest = await _context.MedicalTests.FindAsync(model.Id);
            if (existingTest == null) return NotFound();

            // Update scalar properties
            existingTest.TestName = model.TestName;
            existingTest.Description = model.Description;
            existingTest.HospitalName = model.HospitalName;
            existingTest.HospitalLocation = model.HospitalLocation;
            existingTest.Price = model.Price;
            existingTest.AvailableTime = model.AvailableTime;
            existingTest.DailyAppointmentLimit = model.DailyAppointmentLimit;
            existingTest.MondayLimit = model.MondayLimit;
            existingTest.TuesdayLimit = model.TuesdayLimit;
            existingTest.WednesdayLimit = model.WednesdayLimit;
            existingTest.ThursdayLimit = model.ThursdayLimit;
            existingTest.FridayLimit = model.FridayLimit;
            existingTest.SaturdayLimit = model.SaturdayLimit;
            existingTest.SundayLimit = model.SundayLimit;

            // Update available days
            var formDays = Request.Form["dayList"].ToList();
            existingTest.AvailableDays = formDays.Any() ? string.Join(", ", formDays) : null;

            // --- Image handling ---
            if (removeImage == true)
            {
                if (!string.IsNullOrEmpty(existingTest.ImagePath))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingTest.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }
                existingTest.ImagePath = "";
            }
            else if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "tests");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(existingTest.ImagePath) && existingTest.ImagePath != "")
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingTest.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                existingTest.ImagePath = $"/images/tests/{uniqueFileName}";
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "✅ Medical test updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}