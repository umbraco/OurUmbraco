using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OurUmbraco.Community.GitHub;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Our.Api
{
    public class GitHubController : UmbracoAuthorizedApiController
    {
        [System.Web.Http.HttpGet]
        public List<LabelReport> GetLabelReport()
        {
            var gitHubService = new GitHubService();
            var data = gitHubService.GetLabelReport();
            return data.Where(x => x.NonCompliantLabels.Any() || x.HasRequiredLabels == false).ToList();
        }

        [System.Web.Http.HttpGet]
        public ProjectsCategoriesReport GetGitHubCategoriesProjects()
        {
            var gitHubService = new GitHubService();
            var labelReport = gitHubService.GetLabelReport();

            var pcReport = new ProjectsCategoriesReport { Projects = new List<ProjectLabel>(), Categories = new List<CategoryLabel>() };

            foreach (var report in labelReport.Where(x => x.Projects.Any()))
            {
                foreach (var project in report.Projects.OrderBy(x => x.Name))
                {
                    var projectName = project.Name.Split('/').Skip(1).First().Replace("-", " ").ToLower();
                    projectName = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(projectName);

                    var foundProject = pcReport.Projects.FirstOrDefault(x => x.ProjectName == projectName);
                    if (foundProject != null && foundProject.Repositories.Any(x => x == report.Repository) == false)
                    {
                        foundProject.Repositories.Add(report.Repository);
                    }
                    else
                    {
                        var proj = new ProjectLabel
                        {
                            Repositories = new List<string> { report.Repository },
                            ProjectName = projectName
                        };

                        pcReport.Projects.Add(proj);
                    }
                }
            }

            foreach (var report in labelReport.Where(x => x.Categories.Any()))
            {
                foreach (var category in report.Categories.OrderBy(x => x.Name))
                {
                    var categoryName = category.Name.Split('/').Skip(1).First().Replace("-", " ").ToLower();
                    categoryName = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(categoryName);

                    var foundCategory = pcReport.Categories.FirstOrDefault(x => x.CategoryName == categoryName);
                    if (foundCategory != null && foundCategory.Repositories.Any(x => x == report.Repository) == false)
                    {
                        foundCategory.Repositories.Add(report.Repository);
                    }
                    else
                    {
                        var cat = new CategoryLabel
                        {
                            Repositories = new List<string> { report.Repository },
                            CategoryName = categoryName
                        };

                        pcReport.Categories.Add(cat);
                    }
                }
            }
            
            return pcReport;
        }
    }

    public class ProjectLabel
    {
        public string ProjectName { get; set; }
        public List<string> Repositories { get; set; }
    }

    public class CategoryLabel
    {
        public string CategoryName { get; set; }
        public List<string> Repositories { get; set; }
    }

    public class ProjectsCategoriesReport
    {
        public List<ProjectLabel> Projects { get; set; }
        public List<CategoryLabel> Categories { get; set; }
    }
}