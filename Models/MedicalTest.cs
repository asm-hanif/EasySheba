using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySheba.Models
{
    public class MedicalTest
    {
        public int Id { get; set; }

        [Required]
        public string HospitalAdminId { get; set; }

        [Required]
        [Display(Name = "Hospital/Clinic Name")]
        public string HospitalName { get; set; }

        [Required]
        [Display(Name = "Hospital/Clinic Location")]
        public string HospitalLocation { get; set; }

        [Required]
        [Display(Name = "Test Name")]
        public string TestName { get; set; }

        [Required]
        [Display(Name = "Test Description")]
        public string Description { get; set; }

        [Display(Name = "Test Price")]
        public decimal? Price { get; set; }

        [Display(Name = "Available Days")]
        public string AvailableDays { get; set; }

        [Required]
        [Display(Name = "Available Hours")]
        public string AvailableTime { get; set; }

        [Display(Name = "Test Image")]
        public string ImagePath { get; set; }

        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile ImageFile { get; set; }

        // ✅ Appointment Limit Properties for Medical Tests
        [Display(Name = "Default Daily Limit")]
        public int DailyAppointmentLimit { get; set; } = 50;

        [Display(Name = "Monday Limit")]
        public int MondayLimit { get; set; } = 50;

        [Display(Name = "Tuesday Limit")]
        public int TuesdayLimit { get; set; } = 50;

        [Display(Name = "Wednesday Limit")]
        public int WednesdayLimit { get; set; } = 50;

        [Display(Name = "Thursday Limit")]
        public int ThursdayLimit { get; set; } = 50;

        [Display(Name = "Friday Limit")]
        public int FridayLimit { get; set; } = 50;

        [Display(Name = "Saturday Limit")]
        public int SaturdayLimit { get; set; } = 50;

        [Display(Name = "Sunday Limit")]
        public int SundayLimit { get; set; } = 50;

        // Navigation property
        public List<Appointment> Appointments { get; set; }
    }
}