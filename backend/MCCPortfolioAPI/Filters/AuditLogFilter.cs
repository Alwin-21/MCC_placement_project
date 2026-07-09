using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MCCPortfolioAPI.Data;
using MCCPortfolioAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MCCPortfolioAPI.Filters
{
    public class AuditLogFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _context;

        public AuditLogFilter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            if (resultContext.Exception == null && 
                (resultContext.Result is OkResult || 
                 resultContext.Result is OkObjectResult || 
                 resultContext.Result is CreatedResult || 
                 resultContext.Result is CreatedAtActionResult || 
                 resultContext.Result is NoContentResult))
            {
                var httpContext = context.HttpContext;
                var userEmail = httpContext.User.FindFirstValue(ClaimTypes.Email) ?? httpContext.User.FindFirstValue(ClaimTypes.Name);
                
                if (string.IsNullOrEmpty(userEmail) || userEmail.Equals("admin@mcc.com", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var path = httpContext.Request.Path.ToString().ToLower();
                var method = httpContext.Request.Method.ToUpper();
                string action = "";

                if (path.Contains("/projects"))
                {
                    action = method == "POST" ? "Project Added" : method == "PUT" ? "Project Updated" : method == "DELETE" ? "Project Deleted" : "";
                }
                else if (path.Contains("/experiences"))
                {
                    action = method == "POST" ? "Internship Added" : method == "PUT" ? "Internship Updated" : method == "DELETE" ? "Internship Deleted" : "";
                }
                else if (path.Contains("/skills"))
                {
                    action = method == "POST" ? "Skills Added" : method == "PUT" ? "Skills Updated" : method == "DELETE" ? "Skills Deleted" : "";
                }
                else if (path.Contains("/achievements"))
                {
                    action = method == "POST" ? "Achievement Added" : method == "PUT" ? "Achievement Updated" : method == "DELETE" ? "Achievement Deleted" : "";
                }
                else if (path.Contains("/certifications"))
                {
                    action = method == "POST" ? "Certificate Uploaded" : method == "PUT" ? "Certificate Edited" : method == "DELETE" ? "Certificate Deleted" : "";
                }
                else if (path.Contains("/profiles"))
                {
                    action = (method == "POST" || method == "PUT") ? "Profile Updated" : "";
                }
                else if (path.Contains("/resumes"))
                {
                    action = method == "POST" ? "Resume Uploaded" : method == "DELETE" ? "Resume Deleted" : "";
                }

                if (!string.IsNullOrEmpty(action))
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail || u.Username == userEmail);
                    if (user != null)
                    {
                        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                        
                        var audit = new AuditLog
                        {
                            Action = action,
                            PerformedByEmail = user.Email,
                            Timestamp = DateTime.UtcNow,
                            Details = $"Student {user.FullName} performed {action.ToLower()}.",
                            IpAddress = ip
                        };
                        _context.AuditLogs.Add(audit);

                        var notif = new Notification
                        {
                            Title = action,
                            Message = $"{user.FullName} performed {action.ToLower()}.",
                            Type = "StudentAction",
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow,
                            UserId = user.Id
                        };
                        _context.Notifications.Add(notif);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
