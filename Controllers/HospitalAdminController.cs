using EasySheba.Data;
using EasySheba.Models;
using EasySheba.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Hosting;

namespace EasySheba.Controllers
{
    public class HospitalAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAppointmentLimitService _limitService;

        public HospitalAdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment, IAppointmentLimitService limitService)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _limitService = limitService;
        }

        // ==========================================
        // ✅ DASHBOARD HOME
        // ==========================================
        public IActionResult Index()
        {
            return View();
        }

        // ==========================================
        // ✅ DASHBOARD API METHODS
        // ==========================================
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var admin = await _userManager.GetUserAsync(User);
                var doctorCount = await _context.Doctors.CountAsync(d => d.HospitalAdminId == admin.Id);
                var testCount = await _context.MedicalTests.CountAsync(t => t.HospitalAdminId == admin.Id);
                var beds = await _context.Beds.Where(b => b.HospitalAdminId == admin.Id).ToListAsync();
                var totalBeds = beds.Sum(b => b.TotalBeds);
                var availableBeds = beds.Sum(b => b.AvailableBeds);
                var pendingAppointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.MedicalTest)
                    .Include(a => a.Bed)
                    .Where(a => a.Status == "Pending" && (
                        (a.Doctor != null && a.Doctor.HospitalAdminId == admin.Id) ||
                        (a.MedicalTest != null && a.MedicalTest.HospitalAdminId == admin.Id) ||
                        (a.Bed != null && a.Bed.HospitalAdminId == admin.Id)))
                    .CountAsync();
                return Json(new { doctorCount, testCount, totalBeds, availableBeds, pendingAppointments });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        public async Task<IActionResult> GetRecentAppointments()
        {
            try
            {
                var admin = await _userManager.GetUserAsync(User);
                var appointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.MedicalTest)
                    .Include(a => a.Bed)
                    .Where(a =>
                        (a.Doctor != null && a.Doctor.HospitalAdminId == admin.Id) ||
                        (a.MedicalTest != null && a.MedicalTest.HospitalAdminId == admin.Id) ||
                        (a.Bed != null && a.Bed.HospitalAdminId == admin.Id))
                    .OrderByDescending(a => a.BookedAt)
                    .Take(5)
                    .Select(a => new { a.Id, a.PatientName, a.AppointmentType, a.AppointmentDate, a.Status, a.BookedAt })
                    .ToListAsync();
                return Json(appointments);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        public async Task<IActionResult> GetRecentBlogs()
        {
            try
            {
                var admin = await _userManager.GetUserAsync(User);
                var blogs = await _context.Blogs
                    .Where(b => b.HospitalAdminId == admin.Id)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .Select(b => new
                    {
                        b.Id,
                        b.Headline,
                        b.CreatedAt,
                        MediaCount = !string.IsNullOrEmpty(b.MediaPaths) ? JsonSerializer.Deserialize<List<string>>(b.MediaPaths).Count : 0
                    })
                    .ToListAsync();
                return Json(blogs);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        #region Doctor Management

        public async Task<IActionResult> Doctors()
        {
            var admin = await _userManager.GetUserAsync(User);
            var doctors = await _context.Doctors
                .Where(d => d.HospitalAdminId == admin.Id)
                .OrderByDescending(d => d.Id)
                .ToListAsync();
            return View(doctors);
        }

        [HttpGet]
        public IActionResult AddDoctor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDoctor(Doctor doctor, List<string> dayList,
            int? MondayLimit, int? TuesdayLimit, int? WednesdayLimit, int? ThursdayLimit,
            int? FridayLimit, int? SaturdayLimit, int? SundayLimit)
        {
            ModelState.Remove("HospitalAdminId");
            ModelState.Remove("AvailableDays");
            ModelState.Remove("ImagePath");
            ModelState.Remove("Appointments");

            if (!ModelState.IsValid) return View(doctor);

            var admin = await _userManager.GetUserAsync(User);
            doctor.HospitalAdminId = admin.Id;
            doctor.AvailableDays = (dayList != null && dayList.Count > 0) ? string.Join(", ", dayList) : "Not Selected";
            doctor.MondayLimit = MondayLimit ?? doctor.DailyAppointmentLimit;
            doctor.TuesdayLimit = TuesdayLimit ?? doctor.DailyAppointmentLimit;
            doctor.WednesdayLimit = WednesdayLimit ?? doctor.DailyAppointmentLimit;
            doctor.ThursdayLimit = ThursdayLimit ?? doctor.DailyAppointmentLimit;
            doctor.FridayLimit = FridayLimit ?? doctor.DailyAppointmentLimit;
            doctor.SaturdayLimit = SaturdayLimit ?? doctor.DailyAppointmentLimit;
            doctor.SundayLimit = SundayLimit ?? doctor.DailyAppointmentLimit;

            if (doctor.ImageFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/doctors");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(doctor.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await doctor.ImageFile.CopyToAsync(fileStream);
                }
                doctor.ImagePath = "/images/doctors/" + uniqueFileName;
            }

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            TempData["success"] = "✅ Doctor Added Successfully with Appointment Limits!";
            return RedirectToAction(nameof(Doctors));
        }

        [HttpGet]
        public async Task<IActionResult> EditDoctor(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == id && d.HospitalAdminId == admin.Id);
            if (doctor == null) return NotFound();

            if (!string.IsNullOrEmpty(doctor.AvailableDays))
            {
                ViewBag.SelectedDays = doctor.AvailableDays.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(Doctor doctor, List<string> dayList, bool? removeImage, IFormFile? ImageFile)
        {
            ModelState.Remove("HospitalAdminId");
            ModelState.Remove("AvailableDays");
            ModelState.Remove("ImagePath");
            ModelState.Remove("Appointments");
            ModelState.Remove("ImageFile");

            if (!ModelState.IsValid) return View(doctor);

            var admin = await _userManager.GetUserAsync(User);
            var existingDoctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == doctor.Id && d.HospitalAdminId == admin.Id);
            if (existingDoctor == null) return NotFound();

            // Update scalar properties
            existingDoctor.Name = doctor.Name;
            existingDoctor.Department = doctor.Department;
            existingDoctor.HospitalName = doctor.HospitalName;
            existingDoctor.HospitalLocation = doctor.HospitalLocation;
            existingDoctor.Degrees = doctor.Degrees;
            existingDoctor.Specialist = doctor.Specialist;
            existingDoctor.Experience = doctor.Experience;
            existingDoctor.Fees = doctor.Fees;
            existingDoctor.AvailableTime = doctor.AvailableTime;
            existingDoctor.AvailableDays = (dayList != null && dayList.Count > 0) ? string.Join(", ", dayList) : "Not Selected";
            existingDoctor.DailyAppointmentLimit = doctor.DailyAppointmentLimit;
            existingDoctor.MondayLimit = doctor.MondayLimit > 0 ? doctor.MondayLimit : doctor.DailyAppointmentLimit;
            existingDoctor.TuesdayLimit = doctor.TuesdayLimit > 0 ? doctor.TuesdayLimit : doctor.DailyAppointmentLimit;
            existingDoctor.WednesdayLimit = doctor.WednesdayLimit > 0 ? doctor.WednesdayLimit : doctor.DailyAppointmentLimit;
            existingDoctor.ThursdayLimit = doctor.ThursdayLimit > 0 ? doctor.ThursdayLimit : doctor.DailyAppointmentLimit;
            existingDoctor.FridayLimit = doctor.FridayLimit > 0 ? doctor.FridayLimit : doctor.DailyAppointmentLimit;
            existingDoctor.SaturdayLimit = doctor.SaturdayLimit > 0 ? doctor.SaturdayLimit : doctor.DailyAppointmentLimit;
            existingDoctor.SundayLimit = doctor.SundayLimit > 0 ? doctor.SundayLimit : doctor.DailyAppointmentLimit;

            // Image handling
            if (removeImage == true)
            {
                if (!string.IsNullOrEmpty(existingDoctor.ImagePath))
                {
                    string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingDoctor.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }
                existingDoctor.ImagePath = ""; // empty string to avoid null constraint
            }
            else if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingDoctor.ImagePath))
                {
                    string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingDoctor.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/doctors");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }
                existingDoctor.ImagePath = "/images/doctors/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "✅ Doctor Updated Successfully with Appointment Limits!";
            return RedirectToAction(nameof(Doctors));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == id && d.HospitalAdminId == admin.Id);
            if (doctor == null) return NotFound();

            // Remove references from appointments (set DoctorId to null)
            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id)
                .ToListAsync();
            foreach (var app in appointments)
            {
                app.DoctorId = null;
            }

            // Delete image file
            if (!string.IsNullOrEmpty(doctor.ImagePath))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, doctor.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            TempData["success"] = "🗑 Doctor Deleted Successfully!";
            return RedirectToAction(nameof(Doctors));
        }

        public async Task<IActionResult> DoctorLimits(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == id && d.HospitalAdminId == admin.Id);
            if (doctor == null) return NotFound();

            var weeklyLimits = await _limitService.GetWeeklyLimitsForDoctor(id);
            var today = DateTime.Today;
            var viewModel = new DoctorViewModel
            {
                Doctor = doctor,
                TodayLimit = await _limitService.GetDailyLimitForDoctor(id, today),
                TotalBookedToday = await _limitService.GetBookedCountForDoctor(id, today),
                AvailableSlotsToday = await _limitService.GetAvailableSlotsForDoctor(id, today),
                IsAvailableToday = await _limitService.IsDayAvailableForDoctor(id, today),
                TodayDayName = today.DayOfWeek.ToString(),
                WeeklyLimits = weeklyLimits
            };
            return View(viewModel);
        }

        #endregion

        #region Medical Test Management

        public async Task<IActionResult> MedicalTests()
        {
            var admin = await _userManager.GetUserAsync(User);
            var tests = await _context.MedicalTests
                .Where(t => t.HospitalAdminId == admin.Id)
                .OrderByDescending(t => t.Id)
                .ToListAsync();
            return View(tests);
        }

        [HttpGet]
        public IActionResult AddMedicalTest()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedicalTest(MedicalTest test, List<string> dayList,
            int? MondayLimit, int? TuesdayLimit, int? WednesdayLimit, int? ThursdayLimit,
            int? FridayLimit, int? SaturdayLimit, int? SundayLimit)
        {
            ModelState.Remove("HospitalAdminId");
            ModelState.Remove("AvailableDays");
            ModelState.Remove("ImagePath");
            ModelState.Remove("Appointments");

            if (!ModelState.IsValid) return View(test);

            var admin = await _userManager.GetUserAsync(User);
            test.HospitalAdminId = admin.Id;
            test.AvailableDays = (dayList != null && dayList.Count > 0) ? string.Join(", ", dayList) : "Not Selected";
            test.MondayLimit = MondayLimit ?? test.DailyAppointmentLimit;
            test.TuesdayLimit = TuesdayLimit ?? test.DailyAppointmentLimit;
            test.WednesdayLimit = WednesdayLimit ?? test.DailyAppointmentLimit;
            test.ThursdayLimit = ThursdayLimit ?? test.DailyAppointmentLimit;
            test.FridayLimit = FridayLimit ?? test.DailyAppointmentLimit;
            test.SaturdayLimit = SaturdayLimit ?? test.DailyAppointmentLimit;
            test.SundayLimit = SundayLimit ?? test.DailyAppointmentLimit;

            if (test.ImageFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/tests");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(test.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await test.ImageFile.CopyToAsync(fileStream);
                }
                test.ImagePath = "/images/tests/" + uniqueFileName;
            }

            _context.MedicalTests.Add(test);
            await _context.SaveChangesAsync();
            TempData["success"] = "✅ Medical Test Added Successfully with Appointment Limits!";
            return RedirectToAction(nameof(MedicalTests));
        }

        [HttpGet]
        public async Task<IActionResult> EditMedicalTest(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var test = await _context.MedicalTests
                .FirstOrDefaultAsync(t => t.Id == id && t.HospitalAdminId == admin.Id);
            if (test == null) return NotFound();

            if (!string.IsNullOrEmpty(test.AvailableDays))
            {
                ViewBag.SelectedDays = test.AvailableDays.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            return View(test);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicalTest(MedicalTest test, List<string> dayList, bool? removeImage, IFormFile? ImageFile)
        {
            ModelState.Remove("HospitalAdminId");
            ModelState.Remove("AvailableDays");
            ModelState.Remove("ImagePath");
            ModelState.Remove("ImageFile");
            ModelState.Remove("Appointments");

            if (!ModelState.IsValid) return View(test);

            var admin = await _userManager.GetUserAsync(User);
            var existingTest = await _context.MedicalTests
                .FirstOrDefaultAsync(t => t.Id == test.Id && t.HospitalAdminId == admin.Id);
            if (existingTest == null) return NotFound();

            existingTest.TestName = test.TestName;
            existingTest.Description = test.Description;
            existingTest.HospitalName = test.HospitalName;
            existingTest.HospitalLocation = test.HospitalLocation;
            existingTest.Price = test.Price;
            existingTest.AvailableTime = test.AvailableTime;
            existingTest.AvailableDays = (dayList != null && dayList.Count > 0) ? string.Join(", ", dayList) : "Not Selected";
            existingTest.DailyAppointmentLimit = test.DailyAppointmentLimit;
            existingTest.MondayLimit = test.MondayLimit > 0 ? test.MondayLimit : test.DailyAppointmentLimit;
            existingTest.TuesdayLimit = test.TuesdayLimit > 0 ? test.TuesdayLimit : test.DailyAppointmentLimit;
            existingTest.WednesdayLimit = test.WednesdayLimit > 0 ? test.WednesdayLimit : test.DailyAppointmentLimit;
            existingTest.ThursdayLimit = test.ThursdayLimit > 0 ? test.ThursdayLimit : test.DailyAppointmentLimit;
            existingTest.FridayLimit = test.FridayLimit > 0 ? test.FridayLimit : test.DailyAppointmentLimit;
            existingTest.SaturdayLimit = test.SaturdayLimit > 0 ? test.SaturdayLimit : test.DailyAppointmentLimit;
            existingTest.SundayLimit = test.SundayLimit > 0 ? test.SundayLimit : test.DailyAppointmentLimit;

            // Image handling
            if (removeImage == true)
            {
                if (!string.IsNullOrEmpty(existingTest.ImagePath))
                {
                    string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingTest.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }
                existingTest.ImagePath = "";
            }
            else if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingTest.ImagePath))
                {
                    string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingTest.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/tests");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }
                existingTest.ImagePath = "/images/tests/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "✅ Medical Test Updated Successfully with Appointment Limits!";
            return RedirectToAction(nameof(MedicalTests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicalTest(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var test = await _context.MedicalTests
                .FirstOrDefaultAsync(t => t.Id == id && t.HospitalAdminId == admin.Id);
            if (test == null) return NotFound();

            // Remove references from appointments (set MedicalTestId to null)
            var appointments = await _context.Appointments
                .Where(a => a.MedicalTestId == test.Id)
                .ToListAsync();
            foreach (var app in appointments)
            {
                app.MedicalTestId = null;
            }

            // Delete image file
            if (!string.IsNullOrEmpty(test.ImagePath))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, test.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.MedicalTests.Remove(test);
            await _context.SaveChangesAsync();

            TempData["success"] = "🗑 Medical Test Deleted Successfully!";
            return RedirectToAction(nameof(MedicalTests));
        }

        public async Task<IActionResult> TestLimits(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var test = await _context.MedicalTests
                .FirstOrDefaultAsync(t => t.Id == id && t.HospitalAdminId == admin.Id);
            if (test == null) return NotFound();

            var weeklyLimits = await _limitService.GetWeeklyLimitsForTest(id);
            var today = DateTime.Today;
            var viewModel = new TestViewModel
            {
                MedicalTest = test,
                TodayLimit = await _limitService.GetDailyLimitForTest(id, today),
                TotalBookedToday = await _limitService.GetBookedCountForTest(id, today),
                AvailableSlotsToday = await _limitService.GetAvailableSlotsForTest(id, today),
                IsAvailableToday = await _limitService.IsDayAvailableForTest(id, today),
                TodayDayName = today.DayOfWeek.ToString(),
                WeeklyLimits = weeklyLimits
            };
            return View(viewModel);
        }

        #endregion

        #region Bed Management

        public async Task<IActionResult> Beds()
        {
            var admin = await _userManager.GetUserAsync(User);
            var beds = await _context.Beds
                .Where(b => b.HospitalAdminId == admin.Id)
                .OrderByDescending(b => b.Id)
                .ToListAsync();
            return View(beds);
        }

        [HttpGet]
        public IActionResult AddBed()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBed(Bed bed)
        {
            ModelState.Remove("HospitalAdminId");
            ModelState.Remove("ImagePath");

            if (!ModelState.IsValid) return View(bed);

            var admin = await _userManager.GetUserAsync(User);
            bed.HospitalAdminId = admin.Id;
            bed.AvailableBeds = bed.TotalBeds;

            if (bed.ImageFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/beds");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(bed.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await bed.ImageFile.CopyToAsync(fileStream);
                }
                bed.ImagePath = "/images/beds/" + uniqueFileName;
            }

            _context.Beds.Add(bed);
            await _context.SaveChangesAsync();
            TempData["success"] = "✅ Bed Added Successfully!";
            return RedirectToAction(nameof(Beds));
        }

        [HttpGet]
        public async Task<IActionResult> EditBed(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var bed = await _context.Beds
                .FirstOrDefaultAsync(b => b.Id == id && b.HospitalAdminId == admin.Id);
            if (bed == null) return NotFound();
            return View(bed);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBed(Bed bed, bool? removeImage, IFormFile? ImageFile)
        {
            ModelState.Remove("HospitalAdminId");
            ModelState.Remove("ImagePath");
            ModelState.Remove("ImageFile");
            ModelState.Remove("AvailableBeds");

            if (!ModelState.IsValid) return View(bed);

            var admin = await _userManager.GetUserAsync(User);
            var existingBed = await _context.Beds
                .FirstOrDefaultAsync(b => b.Id == bed.Id && b.HospitalAdminId == admin.Id);
            if (existingBed == null) return NotFound();

            // Update bed properties
            int bedDifference = (bed.TotalBeds ?? 0) - (existingBed.TotalBeds ?? 0);
            existingBed.TotalBeds = bed.TotalBeds;
            existingBed.AvailableBeds += bedDifference;
            if (existingBed.AvailableBeds < 0) existingBed.AvailableBeds = 0;

            existingBed.BedType = bed.BedType;
            existingBed.HospitalName = bed.HospitalName;
            existingBed.HospitalLocation = bed.HospitalLocation;
            existingBed.PricePerDay = bed.PricePerDay;
            existingBed.AcNonAc = bed.AcNonAc;
            existingBed.AttachedBathroom = bed.AttachedBathroom;
            existingBed.TvAvailable = bed.TvAvailable;
            existingBed.WifiAvailable = bed.WifiAvailable;
            existingBed.Description = bed.Description;

            // Image handling
            if (removeImage == true)
            {
                if (!string.IsNullOrEmpty(existingBed.ImagePath))
                {
                    string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingBed.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }
                existingBed.ImagePath = "";
            }
            else if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingBed.ImagePath))
                {
                    string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingBed.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/beds");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }
                existingBed.ImagePath = "/images/beds/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "✅ Bed Updated Successfully!";
            return RedirectToAction(nameof(Beds));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBed(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var bed = await _context.Beds
                .FirstOrDefaultAsync(b => b.Id == id && b.HospitalAdminId == admin.Id);
            if (bed == null) return NotFound();

            // Remove references from appointments (set BedId to null)
            var appointments = await _context.Appointments
                .Where(a => a.BedId == bed.Id)
                .ToListAsync();
            foreach (var app in appointments)
            {
                app.BedId = null;
            }

            // Delete image file
            if (!string.IsNullOrEmpty(bed.ImagePath))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, bed.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Beds.Remove(bed);
            await _context.SaveChangesAsync();

            TempData["success"] = "🗑 Bed Deleted Successfully!";
            return RedirectToAction(nameof(Beds));
        }

        #endregion

        #region Blog Management

        public async Task<IActionResult> Blogs()
        {
            var admin = await _userManager.GetUserAsync(User);
            var blogs = await _context.Blogs
                .Where(b => b.HospitalAdminId == admin.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(blogs);
        }

        [HttpGet]
        public IActionResult AddBlog()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBlog(Blog blog)
        {
            ModelState.Remove("HospitalAdminId");
            ModelState.Remove("CoverImagePath");
            ModelState.Remove("MediaPaths");

            if (!ModelState.IsValid) return View(blog);

            var admin = await _userManager.GetUserAsync(User);
            blog.HospitalAdminId = admin.Id;

            if (blog.CoverImageFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/blogs/covers");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(blog.CoverImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await blog.CoverImageFile.CopyToAsync(fileStream);
                }
                blog.CoverImagePath = "/images/blogs/covers/" + uniqueFileName;
            }

            List<string> mediaPaths = new List<string>();
            if (blog.MediaFiles != null && blog.MediaFiles.Count > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/blogs/media");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                foreach (var file in blog.MediaFiles)
                {
                    if (file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        mediaPaths.Add("/images/blogs/media/" + uniqueFileName);
                    }
                }
            }
            blog.MediaPaths = JsonSerializer.Serialize(mediaPaths);
            blog.CreatedAt = DateTime.Now;
            blog.UpdatedAt = DateTime.Now;

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();
            TempData["success"] = "✅ Blog Added Successfully!";
            return RedirectToAction(nameof(Blogs));
        }

        [HttpGet]
        public async Task<IActionResult> EditBlog(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id && b.HospitalAdminId == admin.Id);
            if (blog == null) return NotFound();

            if (!string.IsNullOrEmpty(blog.MediaPaths))
            {
                ViewBag.MediaPaths = JsonSerializer.Deserialize<List<string>>(blog.MediaPaths);
            }
            return View(blog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBlog(Blog blog, bool? removeCoverImage, IFormFile? CoverImageFile, List<IFormFile>? MediaFiles)
        {
            ModelState.Remove("HospitalAdminId");
            ModelState.Remove("CoverImagePath");
            ModelState.Remove("MediaPaths");
            ModelState.Remove("CoverImageFile");
            ModelState.Remove("MediaFiles");

            if (!ModelState.IsValid) return View(blog);

            var admin = await _userManager.GetUserAsync(User);
            var existingBlog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == blog.Id && b.HospitalAdminId == admin.Id);
            if (existingBlog == null) return NotFound();

            existingBlog.Headline = blog.Headline;
            existingBlog.Description = blog.Description;
            existingBlog.UpdatedAt = DateTime.Now;

            // Cover image handling
            if (removeCoverImage == true)
            {
                if (!string.IsNullOrEmpty(existingBlog.CoverImagePath))
                {
                    string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingBlog.CoverImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }
                existingBlog.CoverImagePath = "";
            }
            else if (CoverImageFile != null && CoverImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingBlog.CoverImagePath))
                {
                    string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingBlog.CoverImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/blogs/covers");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(CoverImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await CoverImageFile.CopyToAsync(fileStream);
                }
                existingBlog.CoverImagePath = "/images/blogs/covers/" + uniqueFileName;
            }

            // Additional media (add new, keep existing)
            if (MediaFiles != null && MediaFiles.Count > 0)
            {
                List<string> existingMedia = new List<string>();
                if (!string.IsNullOrEmpty(existingBlog.MediaPaths))
                {
                    existingMedia = JsonSerializer.Deserialize<List<string>>(existingBlog.MediaPaths);
                }
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/blogs/media");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                foreach (var file in MediaFiles)
                {
                    if (file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        existingMedia.Add("/images/blogs/media/" + uniqueFileName);
                    }
                }
                existingBlog.MediaPaths = JsonSerializer.Serialize(existingMedia);
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "✅ Blog Updated Successfully!";
            return RedirectToAction(nameof(Blogs));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id && b.HospitalAdminId == admin.Id);
            if (blog == null) return NotFound();

            // Delete cover image
            if (!string.IsNullOrEmpty(blog.CoverImagePath))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, blog.CoverImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            // Delete all media files
            if (!string.IsNullOrEmpty(blog.MediaPaths))
            {
                var mediaPaths = JsonSerializer.Deserialize<List<string>>(blog.MediaPaths);
                foreach (var mediaPath in mediaPaths)
                {
                    string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, mediaPath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }
            }

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            TempData["success"] = "🗑 Blog Deleted Successfully!";
            return RedirectToAction(nameof(Blogs));
        }

        #endregion

        #region Appointment Management

        public async Task<IActionResult> Appointments()
        {
            var admin = await _userManager.GetUserAsync(User);
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.MedicalTest)
                .Include(a => a.Bed)
                .Where(a =>
                    (a.Doctor != null && a.Doctor.HospitalAdminId == admin.Id) ||
                    (a.MedicalTest != null && a.MedicalTest.HospitalAdminId == admin.Id) ||
                    (a.Bed != null && a.Bed.HospitalAdminId == admin.Id))
                .OrderByDescending(a => a.BookedAt)
                .ToListAsync();
            return View(appointments);
        }

        public async Task<IActionResult> DoctorAppointments()
        {
            var admin = await _userManager.GetUserAsync(User);
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentType == "Doctor" && a.Doctor.HospitalAdminId == admin.Id)
                .OrderByDescending(a => a.BookedAt)
                .ToListAsync();
            return View(appointments);
        }

        public async Task<IActionResult> TestAppointments()
        {
            var admin = await _userManager.GetUserAsync(User);
            var appointments = await _context.Appointments
                .Include(a => a.MedicalTest)
                .Where(a => a.AppointmentType == "Test" && a.MedicalTest.HospitalAdminId == admin.Id)
                .OrderByDescending(a => a.BookedAt)
                .ToListAsync();
            return View(appointments);
        }

        public async Task<IActionResult> BedAppointments()
        {
            var admin = await _userManager.GetUserAsync(User);
            var appointments = await _context.Appointments
                .Include(a => a.Bed)
                .Where(a => a.AppointmentType == "Bed" && a.Bed.HospitalAdminId == admin.Id)
                .OrderByDescending(a => a.BookedAt)
                .ToListAsync();
            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAppointment(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.MedicalTest)
                .Include(a => a.Bed)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                TempData["error"] = "❌ Appointment not found!";
                return RedirectToAction(nameof(Appointments));
            }

            bool isAuthorized = false;
            if (appointment.AppointmentType == "Doctor" && appointment.Doctor?.HospitalAdminId == admin.Id)
                isAuthorized = true;
            else if (appointment.AppointmentType == "Test" && appointment.MedicalTest?.HospitalAdminId == admin.Id)
                isAuthorized = true;
            else if (appointment.AppointmentType == "Bed" && appointment.Bed?.HospitalAdminId == admin.Id)
                isAuthorized = true;

            if (!isAuthorized)
            {
                TempData["error"] = "❌ You are not authorized!";
                return RedirectToAction(nameof(Appointments));
            }

            try
            {
                if (appointment.AppointmentType == "Bed" && appointment.Bed != null)
                {
                    var bed = await _context.Beds.FindAsync(appointment.BedId);
                    if (bed != null && bed.AvailableBeds > 0)
                    {
                        bed.AvailableBeds--;
                        appointment.Status = "Approved";
                        await _context.SaveChangesAsync();
                        TempData["success"] = "✅ Bed Appointment Approved Successfully!";
                        return RedirectToAction(nameof(Appointments));
                    }
                    else
                    {
                        TempData["error"] = "❌ No beds available!";
                        return RedirectToAction(nameof(Appointments));
                    }
                }

                if (appointment.AppointmentType == "Test" && appointment.MedicalTest != null)
                {
                    var test = appointment.MedicalTest;
                    string dayName = appointment.AppointmentDate.DayOfWeek.ToString();
                    int dailyLimit = dayName switch
                    {
                        "Monday" => test.MondayLimit,
                        "Tuesday" => test.TuesdayLimit,
                        "Wednesday" => test.WednesdayLimit,
                        "Thursday" => test.ThursdayLimit,
                        "Friday" => test.FridayLimit,
                        "Saturday" => test.SaturdayLimit,
                        "Sunday" => test.SundayLimit,
                        _ => test.DailyAppointmentLimit
                    };

                    var currentApprovedCount = await _context.Appointments
                        .CountAsync(a => a.MedicalTestId == appointment.MedicalTestId
                            && a.AppointmentDate.Date == appointment.AppointmentDate.Date
                            && a.Status == "Approved"
                            && a.IsTestCountedTowardsLimit == true);

                    if (currentApprovedCount >= dailyLimit)
                    {
                        TempData["error"] = $"❌ Daily limit reached for this test! ({currentApprovedCount}/{dailyLimit})";
                        return RedirectToAction(nameof(Appointments));
                    }

                    appointment.Status = "Approved";
                    appointment.IsTestCountedTowardsLimit = true;
                    await _context.SaveChangesAsync();
                    TempData["success"] = $"✅ Test Appointment Approved! ({currentApprovedCount + 1}/{dailyLimit} slots filled)";
                    return RedirectToAction(nameof(Appointments));
                }

                if (appointment.AppointmentType == "Doctor" && appointment.Doctor != null)
                {
                    var doctor = appointment.Doctor;
                    string dayName = appointment.AppointmentDate.DayOfWeek.ToString();
                    int dailyLimit = dayName switch
                    {
                        "Monday" => doctor.MondayLimit,
                        "Tuesday" => doctor.TuesdayLimit,
                        "Wednesday" => doctor.WednesdayLimit,
                        "Thursday" => doctor.ThursdayLimit,
                        "Friday" => doctor.FridayLimit,
                        "Saturday" => doctor.SaturdayLimit,
                        "Sunday" => doctor.SundayLimit,
                        _ => doctor.DailyAppointmentLimit
                    };

                    var currentApprovedCount = await _context.Appointments
                        .CountAsync(a => a.DoctorId == appointment.DoctorId
                            && a.AppointmentDate.Date == appointment.AppointmentDate.Date
                            && a.Status == "Approved"
                            && a.IsCountedTowardsLimit == true);

                    if (currentApprovedCount >= dailyLimit)
                    {
                        TempData["error"] = $"❌ Daily limit reached for this doctor! ({currentApprovedCount}/{dailyLimit})";
                        return RedirectToAction(nameof(Appointments));
                    }

                    appointment.Status = "Approved";
                    appointment.IsCountedTowardsLimit = true;
                    await _context.SaveChangesAsync();
                    TempData["success"] = $"✅ Doctor Appointment Approved! ({currentApprovedCount + 1}/{dailyLimit} slots filled)";
                    return RedirectToAction(nameof(Appointments));
                }

                TempData["success"] = "✅ Appointment Approved Successfully!";
            }
            catch (Exception ex)
            {
                TempData["error"] = $"❌ Error: {ex.Message}";
            }
            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectAppointment(int id)
        {
            var admin = await _userManager.GetUserAsync(User);
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.MedicalTest)
                .Include(a => a.Bed)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                TempData["error"] = "❌ Appointment not found!";
                return RedirectToAction(nameof(Appointments));
            }

            bool isAuthorized = false;
            if (appointment.AppointmentType == "Doctor" && appointment.Doctor?.HospitalAdminId == admin.Id)
                isAuthorized = true;
            else if (appointment.AppointmentType == "Test" && appointment.MedicalTest?.HospitalAdminId == admin.Id)
                isAuthorized = true;
            else if (appointment.AppointmentType == "Bed" && appointment.Bed?.HospitalAdminId == admin.Id)
                isAuthorized = true;

            if (!isAuthorized)
            {
                TempData["error"] = "❌ You are not authorized!";
                return RedirectToAction(nameof(Appointments));
            }

            // If a previously approved bed appointment is being rejected, restore bed availability
            if (appointment.AppointmentType == "Bed" && appointment.Status == "Approved" && appointment.Bed != null)
            {
                var bed = await _context.Beds.FindAsync(appointment.BedId);
                if (bed != null)
                {
                    bed.AvailableBeds += 1;
                    if (bed.AvailableBeds > bed.TotalBeds) bed.AvailableBeds = bed.TotalBeds;
                }
            }

            appointment.Status = "Rejected";
            if (appointment.AppointmentType == "Doctor")
                appointment.IsCountedTowardsLimit = false;
            else if (appointment.AppointmentType == "Test")
                appointment.IsTestCountedTowardsLimit = false;

            await _context.SaveChangesAsync();
            TempData["success"] = "❌ Appointment Rejected Successfully!";
            return RedirectToAction(nameof(Appointments));
        }

        [HttpGet]
        public async Task<IActionResult> CheckDoctorLimit(int doctorId, DateTime date)
        {
            var admin = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return NotFound();

            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
                .Select(a => new { a.Id, a.PatientName, a.Status, a.IsCountedTowardsLimit })
                .ToListAsync();

            var approvedCount = appointments.Count(a => a.Status == "Approved" && a.IsCountedTowardsLimit == true);
            string dayName = date.DayOfWeek.ToString();
            var limit = dayName switch
            {
                "Monday" => doctor.MondayLimit,
                "Tuesday" => doctor.TuesdayLimit,
                "Wednesday" => doctor.WednesdayLimit,
                "Thursday" => doctor.ThursdayLimit,
                "Friday" => doctor.FridayLimit,
                "Saturday" => doctor.SaturdayLimit,
                "Sunday" => doctor.SundayLimit,
                _ => doctor.DailyAppointmentLimit
            };

            return Json(new
            {
                type = "Doctor",
                doctorId,
                doctorName = doctor.Name,
                date = date.ToString("yyyy-MM-dd"),
                dayName,
                limit,
                approvedCount,
                availableSlots = limit - approvedCount,
                totalAppointments = appointments.Count,
                appointments
            });
        }

        [HttpGet]
        public async Task<IActionResult> CheckTestLimit(int testId, DateTime date)
        {
            var admin = await _userManager.GetUserAsync(User);
            var test = await _context.MedicalTests.FindAsync(testId);
            if (test == null) return NotFound();

            var appointments = await _context.Appointments
                .Where(a => a.MedicalTestId == testId && a.AppointmentDate.Date == date.Date)
                .Select(a => new { a.Id, a.PatientName, a.Status, a.IsTestCountedTowardsLimit })
                .ToListAsync();

            var approvedCount = appointments.Count(a => a.Status == "Approved" && a.IsTestCountedTowardsLimit == true);
            string dayName = date.DayOfWeek.ToString();
            var limit = dayName switch
            {
                "Monday" => test.MondayLimit,
                "Tuesday" => test.TuesdayLimit,
                "Wednesday" => test.WednesdayLimit,
                "Thursday" => test.ThursdayLimit,
                "Friday" => test.FridayLimit,
                "Saturday" => test.SaturdayLimit,
                "Sunday" => test.SundayLimit,
                _ => test.DailyAppointmentLimit
            };

            return Json(new
            {
                type = "Medical Test",
                testId,
                testName = test.TestName,
                date = date.ToString("yyyy-MM-dd"),
                dayName,
                limit,
                approvedCount,
                availableSlots = limit - approvedCount,
                totalAppointments = appointments.Count,
                appointments
            });
        }

        #endregion
    }
}