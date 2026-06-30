
using BCrypt.Net;
using MCCPortfolioAPI.Data;
using MCCPortfolioAPI.DTOs;
using MCCPortfolioAPI.Entities;
using MCCPortfolioAPI.Models;
using MCCPortfolioAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MCCPortfolioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly JwtService _jwtService;

        private static readonly HashSet<string> AidedDepartments = new(StringComparer.OrdinalIgnoreCase)
        {
            "English", "Tamil", "Languages", "History", "Political Science", "Public Administration",
            "Economics", "Philosophy", "Commerce", "Social Work", "Mathematics", "Statistics",
            "Physics", "Chemistry", "Botany", "Zoology"
        };

        private static readonly HashSet<string> SfsDepartments = new(StringComparer.OrdinalIgnoreCase)
        {
            "English", "Tamil", "Languages", "Journalism", "Social Work", "Commerce",
            "Business Administration", "Communication", "Geography", "Tourism Studies",
            "Mathematics", "Physics", "Chemistry", "Microbiology", "Computer Application (BCA)",
            "Computer Science (B.Sc)", "Computer Science (MCA)", "Visual Communication",
            "Physical Education, Health Education and Sports", "Psychology", "Data Science", "Physical Education"
        };

        public AuthController(
            ApplicationDbContext context,
            JwtService jwtService
        )
        {
            _context = context;

            _jwtService = jwtService;
        }

        // =========================
        // STUDENT REGISTER
        // =========================

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Stream) && !string.IsNullOrWhiteSpace(dto.Department))
            {
                if (AidedDepartments.Contains(dto.Department))
                {
                    dto.Stream = "Aided";
                }
                else if (SfsDepartments.Contains(dto.Department))
                {
                    dto.Stream = "SFS";
                }
            }

            if (string.IsNullOrWhiteSpace(dto.Stream))
            {
                return BadRequest("Stream is required.");
            }

            if (dto.Stream != "Aided" && dto.Stream != "SFS")
            {
                return BadRequest("Stream must be either 'Aided' or 'SFS'.");
            }

            if (string.IsNullOrWhiteSpace(dto.Department))
            {
                return BadRequest("Department is required.");
            }

            if (dto.Stream == "Aided" && !AidedDepartments.Contains(dto.Department))
            {
                return BadRequest($"Department '{dto.Department}' is not valid for Aided stream.");
            }

            if (dto.Stream == "SFS" && !SfsDepartments.Contains(dto.Department))
            {
                return BadRequest($"Department '{dto.Department}' is not valid for SFS stream.");
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (existingUser != null)
            {
                return BadRequest("Email already exists");
            }

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Department = dto.Department,
                Stream = dto.Stream,
                RegisterNumber = dto.RegisterNumber,
                Role = UserRole.Student
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Id = user.Id,
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }

        // =========================
        // STUDENT LOGIN
        // =========================

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            var validPassword = BCrypt.Net.BCrypt.Verify(
                dto.Password,
                user.PasswordHash
            );

            if (!validPassword)
            {
                return Unauthorized("Invalid credentials");
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Id = user.Id,
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }

        // =========================
        // ADMIN LOGIN
        // =========================

        [HttpPost("admin-login")]
        public IActionResult AdminLogin(LoginDto dto)
        {
            var adminEmail = "admin@mcc.com";

            var adminPassword = "admin123";

            if (
                dto.Email != adminEmail ||
                dto.Password != adminPassword
            )
            {
                return Unauthorized(new
                {
                    message = "Invalid Admin Credentials"
                });
            }

            var adminUser = new User
            {
                Id = 999,
                FullName = "Administrator",
                Email = adminEmail,
                Role = UserRole.Admin
            };

            var token = _jwtService.GenerateToken(adminUser);

            return Ok(new
            {
                token = token,

                user = new
                {
                    fullName = adminUser.FullName,

                    email = adminUser.Email,

                    role = "Admin"
                }
            });
        }

        // =========================
        // EXTERNAL GOOGLE/GITHUB LOGIN
        // =========================

        [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLogin(ExternalLoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest("Email is required.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
            {
                user = new User
                {
                    FullName = string.IsNullOrWhiteSpace(dto.FullName) ? "External User" : dto.FullName,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    Department = "Computer Science (B.Sc)",
                    Stream = "SFS",
                    RegisterNumber = "EXT-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    Role = UserRole.Student
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Id = user.Id,
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }
    }
}
