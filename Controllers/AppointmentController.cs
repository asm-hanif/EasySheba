using EasySheba.Data;
using EasySheba.Models;
using EasySheba.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasySheba.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAppointmentLimitService _limitService;

        public AppointmentController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IAppointmentLimitService limitService)
        {
            _context = context;
            _userManager = userManager;
            _limitService = limitService;
        }

        #region Doctor Appointments

        [HttpGet]
        public async Task<IActionResult> CheckDoctorAvailability(int doctorId, DateTime date)
        {
            try
            {
                var isAvailable = await _limitService.IsDayAvailableForDoctor(doctorId, date);
                var availableSlots = await _limitService.GetAvailableSlotsForDoctor(doctorId, date);

                return Json(new
                {
                    success = true,
                    isAvailable = isAvailable,
                    availableSlots = availableSlots,
                    canBook = availableSlots > 0 && isAvailable
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Book(int doctorId)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            var today = DateTime.Today;
            ViewBag.AvailableSlotsToday = await _limitService.GetAvailableSlotsForDoctor(doctorId, today);
            ViewBag.TodayLimit = await _limitService.GetDailyLimitForDoctor(doctorId, today);
            ViewBag.IsAvailableToday = await _limitService.IsDayAvailableForDoctor(doctorId, today);
            ViewBag.Doctor = doctor;
            ViewBag.PatientId = user.Id;

            var appointment = new Appointment
            {
                DoctorId = doctorId,
                AppointmentType = "Doctor",
                PatientId = user.Id,
                PatientName = patient?.FullName ?? "",
                Email = user.Email,
                Phone = "",
                AppointmentDate = DateTime.Today,
                AppointmentTime = "10:00"
            };

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int doctorId, Appointment appointment)
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            var doctor = await _context.Doctors.FindAsync(doctorId);

            if (doctor == null) return NotFound();

            var isDayAvailable = await _limitService.IsDayAvailableForDoctor(doctorId, appointment.AppointmentDate);
            if (!isDayAvailable)
            {
                TempData["error"] = "❌ Doctor is not available on this day!";
                ViewBag.Doctor = doctor;
                return View(appointment);
            }

            var canBook = await _limitService.CanBookDoctorAppointment(doctorId, appointment.AppointmentDate);
            if (!canBook)
            {
                TempData["error"] = $"❌ No slots available on this date!";
                ViewBag.Doctor = doctor;
                return View(appointment);
            }

            appointment.DoctorId = doctorId;
            appointment.AppointmentType = "Doctor";
            appointment.PatientId = user.Id;
            appointment.PatientName = patient?.FullName ?? appointment.PatientName;
            appointment.Email = user.Email;
            appointment.Status = "Pending";
            appointment.BookedAt = DateTime.Now;
            appointment.IsCountedTowardsLimit = false;

            ModelState.Remove("Doctor");
            ModelState.Remove("MedicalTest");
            ModelState.Remove("Bed");
            ModelState.Remove("PatientId");
            ModelState.Remove("Email");

            if (!ModelState.IsValid)
            {
                ViewBag.Doctor = doctor;
                return View(appointment);
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["success"] = "✅ Doctor appointment booked! Waiting for approval.";
            return RedirectToAction(nameof(MyAppointments));
        }

        #endregion

        #region Test Appointments

        [HttpGet]
        public async Task<IActionResult> CheckTestAvailability(int testId, DateTime date)
        {
            try
            {
                var isAvailable = await _limitService.IsDayAvailableForTest(testId, date);
                var availableSlots = await _limitService.GetAvailableSlotsForTest(testId, date);
                var dailyLimit = await _limitService.GetDailyLimitForTest(testId, date);

                return Json(new
                {
                    success = true,
                    isAvailable = isAvailable,
                    availableSlots = availableSlots,
                    dailyLimit = dailyLimit,
                    canBook = availableSlots > 0 && isAvailable
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> BookTest(int testId)
        {
            var test = await _context.MedicalTests.FindAsync(testId);
            if (test == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            var today = DateTime.Today;
            ViewBag.AvailableSlotsToday = await _limitService.GetAvailableSlotsForTest(testId, today);
            ViewBag.TodayLimit = await _limitService.GetDailyLimitForTest(testId, today);
            ViewBag.IsAvailableToday = await _limitService.IsDayAvailableForTest(testId, today);
            ViewBag.Test = test;
            ViewBag.PatientId = user.Id;

            var appointment = new Appointment
            {
                MedicalTestId = testId,
                AppointmentType = "Test",
                PatientId = user.Id,
                PatientName = patient?.FullName ?? "",
                Email = user.Email,
                Phone = "",
                AppointmentDate = DateTime.Today,
                AppointmentTime = "10:00"
            };

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookTest(int testId, Appointment appointment)
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            var test = await _context.MedicalTests.FindAsync(testId);

            if (test == null) return NotFound();

            var isDayAvailable = await _limitService.IsDayAvailableForTest(testId, appointment.AppointmentDate);
            if (!isDayAvailable)
            {
                TempData["error"] = "❌ Test is not available on this day!";
                ViewBag.Test = test;
                return View(appointment);
            }

            var canBook = await _limitService.CanBookTestAppointment(testId, appointment.AppointmentDate);
            if (!canBook)
            {
                TempData["error"] = $"❌ No slots available for this test on this date!";
                ViewBag.Test = test;
                return View(appointment);
            }

            appointment.MedicalTestId = testId;
            appointment.AppointmentType = "Test";
            appointment.PatientId = user.Id;
            appointment.PatientName = patient?.FullName ?? appointment.PatientName;
            appointment.Email = user.Email;
            appointment.Status = "Pending";
            appointment.BookedAt = DateTime.Now;
            appointment.IsTestCountedTowardsLimit = false;

            ModelState.Remove("Doctor");
            ModelState.Remove("MedicalTest");
            ModelState.Remove("Bed");
            ModelState.Remove("PatientId");
            ModelState.Remove("Email");

            if (!ModelState.IsValid)
            {
                ViewBag.Test = test;
                return View(appointment);
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["success"] = "✅ Medical test booked! Waiting for approval.";
            return RedirectToAction(nameof(MyAppointments));
        }

        #endregion

        #region Bed Appointments

        [HttpGet]
        public async Task<IActionResult> BookBed(int bedId)
        {
            var bed = await _context.Beds.FindAsync(bedId);
            if (bed == null) return NotFound();

            if (bed.AvailableBeds <= 0)
            {
                TempData["error"] = "❌ No beds available of this type!";
                return RedirectToAction("Index", "Bed");
            }

            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            var appointment = new Appointment
            {
                BedId = bedId,
                AppointmentType = "Bed",
                PatientId = user.Id,
                PatientName = patient?.FullName ?? "",
                Email = user.Email,
                Phone = ""
            };

            ViewBag.Bed = bed;
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookBed(int bedId, Appointment appointment)
        {
            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            var bed = await _context.Beds.FindAsync(bedId);

            if (bed == null || bed.AvailableBeds <= 0)
            {
                TempData["error"] = "❌ No beds available!";
                return RedirectToAction("Index", "Bed");
            }

            appointment.BedId = bedId;
            appointment.AppointmentType = "Bed";
            appointment.PatientId = user.Id;
            appointment.PatientName = patient?.FullName ?? appointment.PatientName;
            appointment.Email = user.Email;
            appointment.Status = "Pending";
            appointment.BookedAt = DateTime.Now;
            appointment.IsCountedTowardsLimit = false;

            ModelState.Remove("Doctor");
            ModelState.Remove("MedicalTest");
            ModelState.Remove("Bed");
            ModelState.Remove("PatientId");
            ModelState.Remove("Email");

            if (!ModelState.IsValid)
            {
                ViewBag.Bed = bed;
                return View(appointment);
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["success"] = "✅ Bed booked! Waiting for approval.";
            return RedirectToAction(nameof(MyAppointments));
        }

        #endregion

        #region Common Methods

        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);

            var list = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.MedicalTest)
                .Include(a => a.Bed)
                .Where(a => a.PatientId == user.Id)
                .OrderByDescending(a => a.BookedAt)
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.MedicalTest)
                .Include(a => a.Bed)
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == user.Id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // Allow cancelling pending or approved appointments by the patient.
            var appointment = await _context.Appointments
                .Include(a => a.Bed)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == user.Id && (a.Status == "Pending" || a.Status == "Approved"));

            if (appointment == null) return NotFound();

            // If an approved bed appointment is being cancelled, restore bed availability
            if (appointment.AppointmentType == "Bed" && appointment.Status == "Approved" && appointment.Bed != null)
            {
                var bed = await _context.Beds.FindAsync(appointment.BedId);
                if (bed != null)
                {
                    bed.AvailableBeds += 1;
                    if (bed.AvailableBeds > bed.TotalBeds) bed.AvailableBeds = bed.TotalBeds;
                }
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            TempData["success"] = "🗑 Appointment cancelled successfully.";
            return RedirectToAction(nameof(MyAppointments));
        }

        [HttpGet]
        public async Task<IActionResult> Reschedule(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.MedicalTest)
                .Include(a => a.Bed)
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == user.Id && a.Status == "Pending");

            if (appointment == null) return NotFound();

            if (appointment.AppointmentType == "Doctor" && appointment.Doctor != null)
            {
                ViewBag.AvailableSlots = await _limitService.GetAvailableSlotsForDoctor(
                    appointment.DoctorId.Value, appointment.AppointmentDate);
            }
            else if (appointment.AppointmentType == "Test" && appointment.MedicalTest != null)
            {
                ViewBag.AvailableSlots = await _limitService.GetAvailableSlotsForTest(
                    appointment.MedicalTestId.Value, appointment.AppointmentDate);
            }

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, DateTime AppointmentDate, string AppointmentTime)
        {
            var user = await _userManager.GetUserAsync(User);

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.MedicalTest)
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == user.Id && a.Status == "Pending");

            if (appointment == null) return NotFound();

            if (appointment.AppointmentType == "Doctor" && appointment.Doctor != null)
            {
                var canBook = await _limitService.CanBookDoctorAppointment(appointment.DoctorId.Value, AppointmentDate);
                if (!canBook)
                {
                    TempData["error"] = "❌ No slots available on selected date for this doctor!";
                    return RedirectToAction(nameof(Reschedule), new { id });
                }
            }
            else if (appointment.AppointmentType == "Test" && appointment.MedicalTest != null)
            {
                var canBook = await _limitService.CanBookTestAppointment(appointment.MedicalTestId.Value, AppointmentDate);
                if (!canBook)
                {
                    TempData["error"] = "❌ No slots available on selected date for this test!";
                    return RedirectToAction(nameof(Reschedule), new { id });
                }
            }

            appointment.AppointmentDate = AppointmentDate;
            appointment.AppointmentTime = AppointmentTime;
            appointment.Status = "Pending";

            await _context.SaveChangesAsync();

            TempData["success"] = "✅ Appointment rescheduled successfully.";
            return RedirectToAction(nameof(MyAppointments));
        }

        #endregion
    }
}