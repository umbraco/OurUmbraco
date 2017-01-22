using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace OurUmbraco.Our.Models
{
    public class ProjectDetails
    {
        public int Id { get; set; }

        public Guid Guid { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Current version")]
        public string Version { get; set; }

        [Required]
        [Display(Name = "Project category")]
        public string Category { get; set; }

        [Required]
        [Display(Name = "Project description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "License name")]
        public string License { get; set; }

        [Required]
        [Display(Name = "License URL")]
        public string LicenseUrl { get; set; }

        [Display(Name = "Project URL")]
        public string ProjectUrl { get; set; }

        [Display(Name = "Demonstration URL")]
        public string DemonstrationUrl { get; set; }

        [Display(Name = "Source code URL")]
        public string SourceCodeUrl { get; set; }

        [Display(Name = "NuGet package URL")]
        public string NuGetPackageUrl { get; set; }

        [Display(Name = "Bug tracking URL")]
        public string BugTrackingUrl { get; set; }

        [Display(Name = "Google Analytics code")]
        public string GoogleAnalyticsCode { get; set; }

        [Display(Name = "This project is open for collaboration")]
        public bool OpenForCollaboration { get; set; }

        public List<SelectListItem> ProjectCategories { get; set; }
    }
}