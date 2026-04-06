using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasySheba.Models
{
    public class Blog
    {
        public int Id { get; set; }

        [Required]
        public string HospitalAdminId { get; set; }

        [Required]
        [Display(Name = "Blog Headline")]
        public string Headline { get; set; }

        [Required]
        [Display(Name = "Blog Description")]
        public string Description { get; set; }

        [Display(Name = "Cover Image")]
        public string CoverImagePath { get; set; }

        [NotMapped]
        [Display(Name = "Upload Cover Image")]
        public IFormFile CoverImageFile { get; set; }

        // Store multiple media paths as JSON string
        public string MediaPaths { get; set; }

        [NotMapped]
        [Display(Name = "Upload Images/Videos")]
        public List<IFormFile> MediaFiles { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}