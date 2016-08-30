using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OurUmbraco.Our.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Please enter your name")]
        [DisplayName("Name*")]
        public string Name { get; set; }

        [Required (ErrorMessage = "Please enter a valid email address")]
        [EmailAddress]
        [DisplayName("Email*")]
        public string Email { get; set; }

        [Required (ErrorMessage = "Please enter a password")]
        [DisplayName("Password*")]
        public string Password { get; set; }
        
        public string Company { get; set; }
        
        [DisplayName("Twitter alias")]
        public string TwitterAlias { get; set; }
        
        [DisplayName("Where do you live?*")]
        public string Location { get; set; }

        public string Longitude { get; set; }

        public string Latitude { get; set; }
        
        public bool AgreeTerms { get; set; }

        public string Flickr { get; set; }

        public string Bio { get; set; }
    }
}
