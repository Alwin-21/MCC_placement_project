using System;

namespace MCCPortfolioAPI.Entities
{
    public class SavedResume
    {
        public int Id { get; set; }

        public string ResumeTitle { get; set; } = string.Empty;

        public string SelectedTheme { get; set; } = "Professional";

        public string AccentColor { get; set; } = "#18233c";

        public string ResumeDataJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }
}
