using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySheba.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required]
        public string HospitalAdminId { get; set; }

        [Required]
        [Display(Name = "Doctor Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Department")]
        public string Department { get; set; }

        [Required]
        [Display(Name = "Hospital/Clinic Name")]
        public string HospitalName { get; set; }

        [Required]
        [Display(Name = "Hospital/Clinic Location")]
        public string HospitalLocation { get; set; }

        [Required]
        [Display(Name = "Medical Degrees")]
        public string Degrees { get; set; }

        [Required]
        [Display(Name = "Specialist In")]
        public string Specialist { get; set; }

        [Required]
        [Display(Name = "Experience (Years)")]
        public int Experience { get; set; }

        [Required]
        [Display(Name = "Consultation Fee")]
        public decimal Fees { get; set; }

        [Display(Name = "Available Days")]
        public string AvailableDays { get; set; }

        [Required]
        [Display(Name = "Available Hours")]
        public string AvailableTime { get; set; }

        [Display(Name = "Doctor Image")]
        public string ImagePath { get; set; }

        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile ImageFile { get; set; }

        // Appointment Limit Properties
        [Display(Name = "Default Daily Limit")]
        public int DailyAppointmentLimit { get; set; } = 20;

        [Display(Name = "Monday Limit")]
        public int MondayLimit { get; set; } = 20;

        [Display(Name = "Tuesday Limit")]
        public int TuesdayLimit { get; set; } = 20;

        [Display(Name = "Wednesday Limit")]
        public int WednesdayLimit { get; set; } = 20;

        [Display(Name = "Thursday Limit")]
        public int ThursdayLimit { get; set; } = 20;

        [Display(Name = "Friday Limit")]
        public int FridayLimit { get; set; } = 20;

        [Display(Name = "Saturday Limit")]
        public int SaturdayLimit { get; set; } = 20;

        [Display(Name = "Sunday Limit")]
        public int SundayLimit { get; set; } = 20;

        // Navigation property
        public List<Appointment> Appointments { get; set; }
    }
}