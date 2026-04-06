using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace EasySheba.Models
{
    public class Bed
    {
        public int Id { get; set; }

        // Make string properties nullable to match DB values that can be NULL
        public string? BedType { get; set; }
        public string? AcNonAc { get; set; }
        public string? Description { get; set; }
        public string? HospitalName { get; set; }
        public string? HospitalLocation { get; set; }
        public string? ImagePath { get; set; }

        // File upload helper (not mapped to DB)
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        // Numeric and boolean fields (nullable previously)
        public int? TotalBeds { get; set; }
        public int? AvailableBeds { get; set; }
        public decimal? PricePerDay { get; set; }
        public bool? AttachedBathroom { get; set; }
        public bool? TvAvailable { get; set; }
        public bool? WifiAvailable { get; set; }

        // Optional: reference to hospital admin id
        public string? HospitalAdminId { get; set; }
    }
}