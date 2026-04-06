using EasySheba.Models;

namespace EasySheba.Models
{
    public class TestViewModel
    {
        public MedicalTest MedicalTest { get; set; }
        public int AvailableSlotsToday { get; set; }
        public int TotalBookedToday { get; set; }
        public int TodayLimit { get; set; }
        public bool IsAvailableToday { get; set; }
        public string TodayDayName { get; set; }

        public Dictionary<string, int> WeeklyLimits { get; set; }
        public Dictionary<string, int> AvailableSlotsByDay { get; set; }
    }
}