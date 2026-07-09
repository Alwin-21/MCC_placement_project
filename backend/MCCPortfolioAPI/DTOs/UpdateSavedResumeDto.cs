using System.ComponentModel.DataAnnotations;

namespace MCCPortfolioAPI.DTOs
{
    public class UpdateSavedResumeDto
    {
        [Required]
        public string ResumeTitle { get; set; } = string.Empty;

        public string SelectedTheme { get; set; } = "Professional";

        public string AccentColor { get; set; } = "#18233c";

        public string ResumeDataJson { get; set; } = string.Empty;
    }
}
