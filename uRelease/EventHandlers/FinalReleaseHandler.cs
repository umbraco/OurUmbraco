using System;
using System.Globalization;
using System.Linq;
using System.Xml.XPath;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.web;

namespace uRelease.EventHandlers
{
    // Should kick in when a release status is set to "Released"
    public class FinalReleaseHandler : ApplicationBase
    {
        public FinalReleaseHandler()
        {
            Document.BeforePublish += DocumentBeforePublish;
        }

        private void DocumentBeforePublish(Document sender, umbraco.cms.businesslogic.PublishEventArgs e)
        {
            if (sender.ContentType.Alias.ToLowerInvariant() == "Release".ToLowerInvariant())
            {
                var status = sender.GetProperty<string>("releaseStatus");

                var dataTypeId = DataTypeDefinition.GetAll()
                    .First(d => d.Text == "Release Status Dropdown")
                    .DataType.DataTypeDefinitionId;

                var preValues = library.GetPreValues(dataTypeId);
                preValues.MoveNext();

                var preValueIterator = preValues.Current.SelectChildren("preValue", "");
                var preValueId = string.Empty;
                while (preValueIterator.MoveNext())
                {
                    if (preValueIterator.Current.Value == "Released")
                        preValueId = preValueIterator.Current.GetAttribute("id", "");
                }

                if (status == preValueId)
                {
                    var linkToChangeSet = sender.getProperty("linkToChangeset");
                    var hasNuGet = sender.getProperty("hasNuGet");

                    // If these two properties are filled in we must've released it before, so don't go through all this again
                    if (string.IsNullOrWhiteSpace(linkToChangeSet.Value.ToString()) && hasNuGet.Value.ToString() == "0")
                    {
                        linkToChangeSet.Value =
                            string.Format("https://github.com/umbraco/Umbraco-CMS/releases/tag/release-{0}", sender.Text);
                        hasNuGet.Value = 1;

                        var childNamePresets = new[] {"UmbracoCms", "UmbracoCms.AllBinaries", "UmbracoCms.WebPI"};

                        foreach (var childNamePreset in childNamePresets)
                        {
                            var documentName = string.Format("{0}.{1}.zip", childNamePreset, sender.Text);

                            // Don't proceed if child already exists
                            if (sender.Children.Any(child => child.Text == documentName))
                                return;

                            var documentType = DocumentType.GetByAlias("ReleaseDownload");

                            var document = Document.MakeNew(documentName, documentType, new User(0), sender.Id);

                            document.getProperty("downloadLink").Value =
                                string.Format("http://umbracoreleases.blob.core.windows.net/download/{0}", documentName);
                            document.getProperty("uploadDate").Value = DateTime.Now.ToString("yyyy-MM-dd");

                            document.Publish(new User(0));
                            library.UpdateDocumentCache(document.Id);
                        }

                        sender.Publish(new User(0));
                        library.UpdateDocumentCache(sender.Id);
                    }
                }
            }
        }
    }
}
