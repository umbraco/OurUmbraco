using System.ComponentModel.DataAnnotations;

namespace OurUmbraco.Our.Models
{
    public class ProjectCompleteModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Display(Name = "Make live")]
        public bool ProjectLive { get; set; }
        public string ErrorMessage { get; set; }
    }
}