using System.ComponentModel.DataAnnotations;

namespace EasySheba.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        // Appointment Type: Doctor, Test, Bed
        [Required]
        public string AppointmentType { get; set; }

        // Doctor Link
        public int? DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        // Medical Test Link
        public int? MedicalTestId { get; set; }
        public MedicalTest MedicalTest { get; set; }

        // Bed Link
        public int? BedId { get; set; }
        public Bed Bed { get; set; }

        // Patient Link
        public string PatientId { get; set; }

        [Required]
        public string PatientName { get; set; }

        [Required]
        public string Phone { get; set; }

        public string Email { get; set; }

        // Schedule
        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string AppointmentTime { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime BookedAt { get; set; } = DateTime.Now;

        // Add this property
        public bool ReminderSent { get; set; } = false;

        // Track if appointment counts towards daily limit
        // Only Approved appointments count towards the limit
        public bool IsCountedTowardsLimit { get; set; } = false;

        // ✅ For Medical Tests - track if counts towards test limit
        public bool IsTestCountedTowardsLimit { get; set; } = false;
    }
}