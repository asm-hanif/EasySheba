using EasySheba.Data;
using EasySheba.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace EasySheba.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // List all blogs
        public async Task<IActionResult> Index()
        {
            var blogs = await _context.Blogs
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(blogs);
        }

        // Show a single blog post
        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null) return NotFound();

            if (!string.IsNullOrEmpty(blog.MediaPaths))
            {
                try
                {
                    ViewBag.MediaPaths = JsonSerializer.Deserialize<List<string>>(blog.MediaPaths);
                }
                catch
                {
                    ViewBag.MediaPaths = new List<string>();
                }
            }
            else
            {
                ViewBag.MediaPaths = new List<string>();
            }

            return View(blog);
        }

        // ============================
        // EDIT BLOG (GET)
        // ============================
        [HttpGet]
        public async Task<IActionResult> EditBlog(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return NotFound();

            if (!string.IsNullOrEmpty(blog.MediaPaths))
            {
                ViewBag.MediaPaths = JsonSerializer.Deserialize<List<string>>(blog.MediaPaths);
            }

            return View(blog);
        }

        // ============================
        // EDIT BLOG (POST)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBlog(Blog model, bool? removeCoverImage, IFormFile? CoverImageFile, List<IFormFile>? MediaFiles)
        {
            ModelState.Remove("CoverImagePath");
            ModelState.Remove("MediaPaths");

            if (!ModelState.IsValid) return View(model);

            var existingBlog = await _context.Blogs.FindAsync(model.Id);
            if (existingBlog == null) return NotFound();

            // Update text fields
            existingBlog.Headline = model.Headline;
            existingBlog.Description = model.Description;
            existingBlog.UpdatedAt = DateTime.Now;

            // --- Cover image handling ---
            if (removeCoverImage == true)
            {
                if (!string.IsNullOrEmpty(existingBlog.CoverImagePath))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingBlog.CoverImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }
                existingBlog.CoverImagePath = "";
            }
            else if (CoverImageFile != null && CoverImageFile.Length > 0)
            {
                // Delete old cover
                if (!string.IsNullOrEmpty(existingBlog.CoverImagePath))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingBlog.CoverImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "blogs", "covers");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(CoverImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await CoverImageFile.CopyToAsync(stream);
                }
                existingBlog.CoverImagePath = $"/images/blogs/covers/{uniqueFileName}";
            }

            // --- Additional media handling (add new, never delete existing ones in this edit) ---
            if (MediaFiles != null && MediaFiles.Count > 0)
            {
                List<string> existingMedia = new List<string>();
                if (!string.IsNullOrEmpty(existingBlog.MediaPaths))
                {
                    existingMedia = JsonSerializer.Deserialize<List<string>>(existingBlog.MediaPaths);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "blogs", "media");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                foreach (var file in MediaFiles)
                {
                    if (file.Length > 0)
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        existingMedia.Add($"/images/blogs/media/{uniqueFileName}");
                    }
                }
                existingBlog.MediaPaths = JsonSerializer.Serialize(existingMedia);
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "✅ Blog updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}