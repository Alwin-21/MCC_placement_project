
using BCrypt.Net;
using MCCPortfolioAPI.Data;
using MCCPortfolioAPI.DTOs;
using MCCPortfolioAPI.Entities;
using MCCPortfolioAPI.Models;
using MCCPortfolioAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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

        private readonly IEmailService _emailService;

        public AuthController(
            ApplicationDbContext context,
            JwtService jwtService,
            IEmailService emailService
        )
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // =========================
        // STUDENT REGISTER
        // =========================

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.EndsWith("@mcc.edu.in", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Registration is restricted to Madras Christian College email addresses ending with '@mcc.edu.in'.");
            }

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

            // Generate unique username
            string username;
            do
            {
                var cleanName = dto.FullName.Replace(" ", "").ToLower();
                username = $"mcc_{cleanName}_{new Random().Next(100, 999)}";
            } while (await _context.Users.AnyAsync(u => u.Username == username));

            // Generate secure random temporary password
            var temporaryPassword = GenerateTemporaryPassword();

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),
                IsTemporaryPassword = true,
                Department = dto.Department,
                Stream = dto.Stream,
                RegisterNumber = dto.RegisterNumber,
                Role = UserRole.Student
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send Email Credentials
            var subject = "Your Madras Christian College Portfolio Credentials";
            var body = $"Dear {user.FullName},\n\n" +
                       $"Your student portfolio account has been created successfully!\n\n" +
                       $"Please use the following credentials to access the placement platform:\n" +
                       $"  Username: {username}\n" +
                       $"  Temporary Password: {temporaryPassword}\n\n" +
                       $"Instructions:\n" +
                       $"1. Visit the portal login page.\n" +
                       $"2. Enter your generated username and temporary password.\n" +
                       $"3. Upon first login, you will be prompted to change your temporary password to a secure permanent one.\n\n" +
                       $"Best regards,\n" +
                       $"Placement Cell, Madras Christian College";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            var log = new AuditLog
            {
                Action = "Student Registration",
                PerformedByEmail = user.Email,
                Timestamp = DateTime.UtcNow,
                Details = $"Student {user.FullName} registered successfully with temporary credentials.",
                IpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1"
            };
            _context.AuditLogs.Add(log);

            var notif = new Notification
            {
                Title = "Student Registration",
                Message = $"{user.FullName} registered.",
                Type = "StudentAction",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            };
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Registration successful. Login credentials have been sent to your registered email." });
        }

        // =========================
        // STUDENT LOGIN
        // =========================

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var searchKey = !string.IsNullOrWhiteSpace(dto.Username) ? dto.Username : dto.Email;
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Username == searchKey || x.Email == searchKey);

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            if (!user.IsActive)
            {
                return Unauthorized("Your account has been deactivated by the administrator.");
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

            var log = new AuditLog
            {
                Action = "Student Login",
                PerformedByEmail = user.Email,
                Timestamp = DateTime.UtcNow,
                Details = $"Student {user.FullName} logged in successfully.",
                IpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1"
            };
            _context.AuditLogs.Add(log);

            var notif = new Notification
            {
                Title = "Student Login",
                Message = $"{user.FullName} logged in.",
                Type = "StudentAction",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            };
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                Id = user.Id,
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsTemporaryPassword = user.IsTemporaryPassword
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email || u.Username == email);
                if (user != null)
                {
                    var log = new AuditLog
                    {
                        Action = "Student Logout",
                        PerformedByEmail = user.Email,
                        Timestamp = DateTime.UtcNow,
                        Details = $"Student {user.FullName} logged out.",
                        IpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1"
                    };
                    _context.AuditLogs.Add(log);

                    var notif = new Notification
                    {
                        Title = "Student Logout",
                        Message = $"{user.FullName} logged out.",
                        Type = "StudentAction",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        UserId = user.Id
                    };
                    _context.Notifications.Add(notif);
                    await _context.SaveChangesAsync();
                }
            }
            return Ok(new { success = true });
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

            if (!dto.Email.EndsWith("@mcc.edu.in", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("External login is restricted to Madras Christian College email addresses ending with '@mcc.edu.in'.");
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
                Role = user.Role.ToString(),
                IsTemporaryPassword = user.IsTemporaryPassword
            });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            {
                return BadRequest("Password must be at least 6 characters.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.IsTemporaryPassword = false;

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Password updated successfully." });
        }
    }
}
