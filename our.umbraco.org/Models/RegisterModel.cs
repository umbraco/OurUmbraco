using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace our.Models
{
    public class RegisterModel
    {
        [Required]
        public string Avatar { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm")]
        [Compare("Password")]
        public string RepeatPassword { get; set; }

        public string Company { get; set; }

        public string Bio { get; set; }

        [Display(Name = "Twitter")]
        public string TwitterAlias { get; set; }

        [Display(Name = "GitHub")]
        public string GitHubUsername { get; set; }

        public string Location { get; set; }
    }
}
