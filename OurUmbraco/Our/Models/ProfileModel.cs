using System.ComponentModel.DataAnnotations;

namespace OurUmbraco.Our.Models
{
    public class ProfileModel
    {
        public string Avatar { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Password { get; set; }

        [Display(Name="Confirm")]
        public string RepeatPassword { get; set; }

        public string Company { get; set; }

        public string Bio { get; set; }

        [Display(Name = "Twitter alias")]
        public string TwitterAlias { get; set; }

        public string Location { get; set; }

        [Display(Name = "GitHub username")]
        public string GitHubUsername { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }
    }
}
