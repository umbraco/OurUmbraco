using System.ComponentModel.DataAnnotations;
using System.Web;

namespace OurUmbraco.Our.Models
{
    public class ScreenshotModel
    {
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public HttpPostedFileBase File { get; set; }
    }
}
