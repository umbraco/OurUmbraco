using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace our.Models
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

        public string TwitterAlias { get; set; }

        public string GitHubUsername { get; set; }

        public string Location { get; set; }


    }
}
