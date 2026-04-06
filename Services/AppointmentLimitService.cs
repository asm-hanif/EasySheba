using EasySheba.Data;
using EasySheba.Models;
using Microsoft.EntityFrameworkCore;

namespace EasySheba.Services
{
    public interface IAppointmentLimitService
    {
        // Doctor methods
        Task<int> GetBookedCountForDoctor(int doctorId, DateTime date);
        Task<int> GetAvailableSlotsForDoctor(int doctorId, DateTime date);
        Task<int> GetDailyLimitForDoctor(int doctorId, DateTime date);
        Task<bool> CanBookDoctorAppointment(int doctorId, DateTime date);
        Task<Dictionary<string, int>> GetWeeklyLimitsForDoctor(int doctorId);
        Task<bool> IsDayAvailableForDoctor(int doctorId, DateTime date);

        // Medical Test methods
        Task<int> GetBookedCountForTest(int testId, DateTime date);
        Task<int> GetAvailableSlotsForTest(int testId, DateTime date);
        Task<int> GetDailyLimitForTest(int testId, DateTime date);
        Task<bool> CanBookTestAppointment(int testId, DateTime date);
        Task<Dictionary<string, int>> GetWeeklyLimitsForTest(int testId);
        Task<bool> IsDayAvailableForTest(int testId, DateTime date);
    }

    public class AppointmentLimitService : IAppointmentLimitService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentLimitService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Doctor Methods

        public async Task<int> GetBookedCountForDoctor(int doctorId, DateTime date)
        {
            return await _context.Appointments
                .CountAsync(a => a.DoctorId == doctorId
                    && a.AppointmentDate.Date == date.Date
                    && a.Status == "Approved"
                    && a.IsCountedTowardsLimit == true);
        }

        public async Task<int> GetDailyLimitForDoctor(int doctorId, DateTime date)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return 0;

            string dayName = date.DayOfWeek.ToString();

            return dayName switch
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
        }

        public async Task<int> GetAvailableSlotsForDoctor(int doctorId, DateTime date)
        {
            var limit = await GetDailyLimitForDoctor(doctorId, date);
            var booked = await GetBookedCountForDoctor(doctorId, date);
            return Math.Max(0, limit - booked);
        }

        public async Task<bool> CanBookDoctorAppointment(int doctorId, DateTime date)
        {
            var isDayAvailable = await IsDayAvailableForDoctor(doctorId, date);
            if (!isDayAvailable) return false;

            var available = await GetAvailableSlotsForDoctor(doctorId, date);
            return available > 0;
        }

        public async Task<bool> IsDayAvailableForDoctor(int doctorId, DateTime date)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null || string.IsNullOrEmpty(doctor.AvailableDays))
                return false;

            string dayName = date.DayOfWeek.ToString();
            var availableDaysList = doctor.AvailableDays.Split(", ", StringSplitOptions.RemoveEmptyEntries);

            return availableDaysList.Contains(dayName);
        }

        public async Task<Dictionary<string, int>> GetWeeklyLimitsForDoctor(int doctorId)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return new Dictionary<string, int>();

            return new Dictionary<string, int>
            {
                { "Monday", doctor.MondayLimit },
                { "Tuesday", doctor.TuesdayLimit },
                { "Wednesday", doctor.WednesdayLimit },
                { "Thursday", doctor.ThursdayLimit },
                { "Friday", doctor.FridayLimit },
                { "Saturday", doctor.SaturdayLimit },
                { "Sunday", doctor.SundayLimit }
            };
        }

        #endregion

        #region Medical Test Methods

        public async Task<int> GetBookedCountForTest(int testId, DateTime date)
        {
            return await _context.Appointments
                .CountAsync(a => a.MedicalTestId == testId
                    && a.AppointmentDate.Date == date.Date
                    && a.Status == "Approved"
                    && a.IsTestCountedTowardsLimit == true);
        }

        public async Task<int> GetDailyLimitForTest(int testId, DateTime date)
        {
            var test = await _context.MedicalTests.FindAsync(testId);
            if (test == null) return 0;

            string dayName = date.DayOfWeek.ToString();

            return dayName switch
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
        }

        public async Task<int> GetAvailableSlotsForTest(int testId, DateTime date)
        {
            var limit = await GetDailyLimitForTest(testId, date);
            var booked = await GetBookedCountForTest(testId, date);
            return Math.Max(0, limit - booked);
        }

        public async Task<bool> CanBookTestAppointment(int testId, DateTime date)
        {
            var isDayAvailable = await IsDayAvailableForTest(testId, date);
            if (!isDayAvailable) return false;

            var available = await GetAvailableSlotsForTest(testId, date);
            return available > 0;
        }

        public async Task<bool> IsDayAvailableForTest(int testId, DateTime date)
        {
            var test = await _context.MedicalTests.FindAsync(testId);
            if (test == null || string.IsNullOrEmpty(test.AvailableDays))
                return false;

            string dayName = date.DayOfWeek.ToString();
            var availableDaysList = test.AvailableDays.Split(", ", StringSplitOptions.RemoveEmptyEntries);

            return availableDaysList.Contains(dayName);
        }

        public async Task<Dictionary<string, int>> GetWeeklyLimitsForTest(int testId)
        {
            var test = await _context.MedicalTests.FindAsync(testId);
            if (test == null) return new Dictionary<string, int>();

            return new Dictionary<string, int>
            {
                { "Monday", test.MondayLimit },
                { "Tuesday", test.TuesdayLimit },
                { "Wednesday", test.WednesdayLimit },
                { "Thursday", test.ThursdayLimit },
                { "Friday", test.FridayLimit },
                { "Saturday", test.SaturdayLimit },
                { "Sunday", test.SundayLimit }
            };
        }

        #endregion
    }
}