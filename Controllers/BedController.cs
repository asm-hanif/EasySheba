using EasySheba.Data;
using EasySheba.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

namespace EasySheba.Controllers
{
    public class BedController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BedController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var beds = await _context.Beds.ToListAsync();
            return View(beds);
        }

        // ============================
        // EDIT BED (GET)
        // ============================
        [HttpGet]
        public async Task<IActionResult> EditBed(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed == null) return NotFound();
            return View(bed);
        }

        // ============================
        // EDIT BED (POST)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBed(Bed model, bool? removeImage, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid) return View(model);

            var existingBed = await _context.Beds.FindAsync(model.Id);
            if (existingBed == null) return NotFound();

            // Update scalar properties
            existingBed.HospitalName = model.HospitalName;
            existingBed.HospitalLocation = model.HospitalLocation;
            existingBed.BedType = model.BedType;
            existingBed.AcNonAc = model.AcNonAc;
            existingBed.TotalBeds = model.TotalBeds;
            existingBed.PricePerDay = model.PricePerDay;
            existingBed.AttachedBathroom = model.AttachedBathroom;
            existingBed.TvAvailable = model.TvAvailable;
            existingBed.WifiAvailable = model.WifiAvailable;
            existingBed.Description = model.Description;

            // Do NOT change AvailableBeds – it's managed by the system

            // --- Image handling ---
            if (removeImage == true)
            {
                if (!string.IsNullOrEmpty(existingBed.ImagePath))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingBed.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }
                existingBed.ImagePath = "";
            }
            else if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "beds");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(existingBed.ImagePath) && existingBed.ImagePath != "")
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingBed.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                existingBed.ImagePath = $"/images/beds/{uniqueFileName}";
            }

            // If TotalBeds changed, adjust AvailableBeds accordingly
            int bedDifference = (model.TotalBeds ?? 0) - (existingBed.TotalBeds ?? 0);
            existingBed.AvailableBeds += bedDifference;
            if (existingBed.AvailableBeds < 0) existingBed.AvailableBeds = 0;

            await _context.SaveChangesAsync();

            TempData["success"] = "✅ Bed updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}