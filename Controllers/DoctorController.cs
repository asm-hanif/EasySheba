using EasySheba.Data;
using EasySheba.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace EasySheba.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================
        // SHOW ALL DOCTORS
        // ============================
        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Doctors.ToListAsync();
            return View(doctors);
        }

        // ============================
        // BOOK APPOINTMENT FORM (GET)
        // ============================
        [HttpGet]
        public async Task<IActionResult> Book(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();
            ViewBag.Doctor = doctor;
            return View();
        }

        // ============================
        // BOOK APPOINTMENT FORM (POST)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(Appointment appointment)
        {
            if (!ModelState.IsValid) return View(appointment);
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            TempData["success"] = "✅ Appointment booked successfully!";
            return RedirectToAction("Index");
        }

        // ============================
        // EDIT DOCTOR (GET)
        // ============================
        [HttpGet]
        public async Task<IActionResult> EditDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            var selectedDays = string.IsNullOrEmpty(doctor.AvailableDays)
                ? new System.Collections.Generic.List<string>()
                : doctor.AvailableDays.Split(", ").ToList();
            ViewBag.SelectedDays = selectedDays;

            return View(doctor);
        }

        // ============================
        // EDIT DOCTOR (POST)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(Doctor model, bool? removeImage, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                var selectedDays = string.IsNullOrEmpty(model.AvailableDays)
                    ? new System.Collections.Generic.List<string>()
                    : model.AvailableDays.Split(", ").ToList();
                ViewBag.SelectedDays = selectedDays;
                return View(model);
            }

            var doctor = await _context.Doctors.FindAsync(model.Id);
            if (doctor == null) return NotFound();

            // Update scalar properties
            doctor.Name = model.Name;
            doctor.Department = model.Department;
            doctor.Degrees = model.Degrees;
            doctor.Specialist = model.Specialist;
            doctor.Experience = model.Experience;
            doctor.Fees = model.Fees;
            doctor.HospitalName = model.HospitalName;
            doctor.HospitalLocation = model.HospitalLocation;
            doctor.AvailableTime = model.AvailableTime;
            doctor.DailyAppointmentLimit = model.DailyAppointmentLimit;
            doctor.MondayLimit = model.MondayLimit;
            doctor.TuesdayLimit = model.TuesdayLimit;
            doctor.WednesdayLimit = model.WednesdayLimit;
            doctor.ThursdayLimit = model.ThursdayLimit;
            doctor.FridayLimit = model.FridayLimit;
            doctor.SaturdayLimit = model.SaturdayLimit;
            doctor.SundayLimit = model.SundayLimit;

            // Update available days
            var formDays = Request.Form["dayList"].ToList();
            doctor.AvailableDays = formDays.Any() ? string.Join(", ", formDays) : null;

            // --- Image handling ---
            if (removeImage == true)
            {
                // Delete existing image file
                if (!string.IsNullOrEmpty(doctor.ImagePath))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", doctor.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }
                // Set to empty string because column does not accept NULL
                doctor.ImagePath = "";
            }
            else if (ImageFile != null && ImageFile.Length > 0)
            {
                // Save new image
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "doctors");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                // Delete old image if exists
                if (!string.IsNullOrEmpty(doctor.ImagePath) && doctor.ImagePath != "")
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", doctor.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                doctor.ImagePath = $"/images/doctors/{uniqueFileName}";
            }

            _context.Update(doctor);
            await _context.SaveChangesAsync();

            TempData["success"] = "Doctor updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}