using System.ComponentModel.DataAnnotations;

namespace OurUmbraco.Our.Models
{
    public class ProfileNotificationModel
    {
        [Display(Name = "Don't bug me")]
        public bool DontBug { get; set; }
    }
}
