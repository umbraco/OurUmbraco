using System.Collections.Generic;
using OurUmbraco.Wiki.BusinessLogic;

namespace OurUmbraco.Our.Models
{
    public class EditScreenshotModel
    {
        public ScreenshotModel UploadFile { get; set; }
        public List<WikiFile> AvailableFiles { get; set; }
    }
}
