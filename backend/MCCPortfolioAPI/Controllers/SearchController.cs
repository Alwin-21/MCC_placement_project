using MCCPortfolioAPI.Data;
using MCCPortfolioAPI.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MCCPortfolioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> SearchStudents(string query)
        {
            query = (query ?? "").Trim().ToLower();

            var studentsQuery = _context.Users
                .Join(_context.Profiles,
                    u => u.Id,
                    p => p.UserId,
                    (u, p) => new { User = u, Profile = p })
                .Where(x => x.Profile.IsApproved && x.User.IsActive && x.User.Role == UserRole.Student);

            if (!string.IsNullOrEmpty(query))
            {
                studentsQuery = studentsQuery.Where(x =>
                    x.User.FullName.ToLower().Contains(query) ||
                    x.User.Department.ToLower().Contains(query) ||
                    x.Profile.CurrentLocation.ToLower().Contains(query) ||
                    _context.Skills.Any(s =>
                        s.UserId == x.User.Id &&
                        s.Name.ToLower().Contains(query)
                    )
                );
            }

            var results = await studentsQuery
                .Select(x => new
                {
                    x.User.Id,
                    x.User.FullName,
                    x.User.Email,
                    x.User.Department,
                    CurrentLocation = x.Profile.CurrentLocation
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}