using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MCCPortfolioAPI.Data;
using MCCPortfolioAPI.DTOs;
using MCCPortfolioAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MCCPortfolioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavedResumesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SavedResumesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateSavedResume(CreateSavedResumeDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Unauthorized();

            var savedResume = new SavedResume
            {
                ResumeTitle = dto.ResumeTitle,
                SelectedTheme = dto.SelectedTheme,
                AccentColor = dto.AccentColor,
                ResumeDataJson = dto.ResumeDataJson,
                UserId = int.Parse(userIdStr),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SavedResumes.Add(savedResume);
            await _context.SaveChangesAsync();

            return Ok(savedResume);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetSavedResumes()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Unauthorized();

            var resumes = await _context.SavedResumes
                .Where(x => x.UserId == int.Parse(userIdStr))
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return Ok(resumes);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSavedResume(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Unauthorized();

            var resume = await _context.SavedResumes
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == int.Parse(userIdStr));

            if (resume == null) return NotFound();

            return Ok(resume);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSavedResume(int id, UpdateSavedResumeDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Unauthorized();

            var resume = await _context.SavedResumes
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == int.Parse(userIdStr));

            if (resume == null) return NotFound();

            resume.ResumeTitle = dto.ResumeTitle;
            resume.SelectedTheme = dto.SelectedTheme;
            resume.AccentColor = dto.AccentColor;
            resume.ResumeDataJson = dto.ResumeDataJson;
            resume.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(resume);
        }

        [Authorize]
        [HttpPost("{id}/duplicate")]
        public async Task<IActionResult> DuplicateSavedResume(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Unauthorized();

            var existing = await _context.SavedResumes
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == int.Parse(userIdStr));

            if (existing == null) return NotFound();

            var duplicate = new SavedResume
            {
                ResumeTitle = $"{existing.ResumeTitle} (Copy)",
                SelectedTheme = existing.SelectedTheme,
                AccentColor = existing.AccentColor,
                ResumeDataJson = existing.ResumeDataJson,
                UserId = existing.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SavedResumes.Add(duplicate);
            await _context.SaveChangesAsync();

            return Ok(duplicate);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSavedResume(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Unauthorized();

            var resume = await _context.SavedResumes
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == int.Parse(userIdStr));

            if (resume == null) return NotFound();

            _context.SavedResumes.Remove(resume);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Resume deleted successfully." });
        }
    }
}
