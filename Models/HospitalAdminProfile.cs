using System.ComponentModel.DataAnnotations;

namespace EasySheba.Models
{
    public class HospitalAdminProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Hospital Name")]
        public string HospitalName { get; set; }

        [Required]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [Display(Name = "Documents")]
        public string DocumentsPath { get; set; }

        [Display(Name = "Is Approved")]
        public bool IsApproved { get; set; }
    }
}