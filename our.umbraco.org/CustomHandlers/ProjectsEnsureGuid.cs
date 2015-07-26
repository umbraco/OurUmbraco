using System;
using System.Collections.Generic;
using OurUmbraco.Powers.Library;
using OurUmbraco.Wiki.BusinessLogic;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;

namespace our.CustomHandlers
{
    public class ProjectsEnsureGuid : ApplicationEventHandler
    {
       
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            Document.BeforePublish += new Document.PublishEventHandler(Document_BeforePublish);
        }

        void Document_BeforePublish(Document sender, umbraco.cms.businesslogic.PublishEventArgs e)
        {
            if (sender.ContentType.Alias == "Project")
            {
                //ensure that packages have a guid
                if (sender.getProperty("packageGuid") != null && String.IsNullOrEmpty(sender.getProperty("packageGuid").Value.ToString()))
                {
                    sender.getProperty("packageGuid").Value = Guid.NewGuid().ToString();
                    sender.Save();
                }

                //if the score is above the minimum, set the approved variable
                int score = Xslt.Score(sender.Id, "powersProject");
                if (score >= 15)
                {
                    sender.getProperty("approved").Value = true;
                    sender.Save();
                }

                //this ensures the package stores it's compatible versions on the node itself, so we save a ton of sql calls
                if (sender.getProperty("compatibleVersions") != null)
                {
                    List<string> compatibleVersions = new List<string>();
                    foreach (WikiFile wf in WikiFile.CurrentFiles(sender.Id))
                    {
                        if (wf.FileType == "package")
                        {
                            foreach (var ver in wf.Versions)
                            {
                                if (!compatibleVersions.Contains(ver.Version))
                                {
                                    compatibleVersions.Add(ver.Version);
                                }
                            }
                        }
                    }

                    string _compatibleVersions = string.Join(",", compatibleVersions.ToArray());
                    sender.getProperty("compatibleVersions").Value = "saved," + _compatibleVersions;
                }

            }
        }
    }
}