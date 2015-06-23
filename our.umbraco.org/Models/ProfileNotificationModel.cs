using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace our.Models
{
    public class ProfileNotificationModel
    {
        [Display(Name = "Don't bug me")]
        public bool DontBug { get; set; }
    }
}
