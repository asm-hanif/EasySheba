using System.ComponentModel.DataAnnotations;

namespace EasySheba.Models
{
    public class PatientProfile
    {
        [Key]
        public int Id { get; set; }

        // Link with Identity User
        public string UserId { get; set; }

        [Required]
        public string FullName { get; set; }
    }
}
