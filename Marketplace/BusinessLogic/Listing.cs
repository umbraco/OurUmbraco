using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marketplace.Interfaces;
using Marketplace.Providers;

namespace Marketplace.BusinessLogic
{
    public static class Listing
    {



        public static IEligible CheckEligibility(IListingItem project)
        {
            //conditions of listing
            var hasPackageFiles = (project.PackageFile.Where(x => !x.Archived).Count() > 0);
            var hasDocumentationFiles = (project.DocumentationFile.Where(x => !x.Archived).Count() > 0);
            var hasScreenShots = project.ScreenShots.Count() > 0;

            //not sure how to do this so it makes any sense.  Takes into account that it has to have more than just <p></p>
            var hasDescription = project.Description.Length > 8;

            var eligible = new Eligible();

            if (hasPackageFiles && hasDocumentationFiles && hasScreenShots && hasScreenShots && hasDescription)
            {
                eligible.Message = "<p>Your project has met all of the basic listing requirements.</p>";
                eligible.IsEligible = true;
                return eligible;
            }

            eligible.IsEligible = false;
            eligible.Message = "<p>You are not able to send your package live at this time as it has the following problems:</p><ul>";
            if (!hasPackageFiles)
            {
                eligible.Message += "<li>You have not uploaded any package files.</li>";
            }
            if (!hasDocumentationFiles)
            {
                eligible.Message += "<li>You have not uploaded any documentation files.</li>";
            }
            if (!hasScreenShots)
            {
                eligible.Message += "<li>You have not uploaded any screenshots.</li>";
            }
            if (!hasDescription)
            {
                eligible.Message += "<li>Your description is either blank or too short.</li>";
            }

            eligible.Message += "</ul><p>Please recitify and try again</p>";

            return eligible;
            

        }
    }
}
