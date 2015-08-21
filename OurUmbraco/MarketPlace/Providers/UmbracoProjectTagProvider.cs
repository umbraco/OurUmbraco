using System.Collections.Generic;
using System.Linq;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.MarketPlace.NodeListing;
using umbraco.cms.businesslogic.Tags;

namespace OurUmbraco.MarketPlace.Providers
{
    public class UmbracoProjectTagProvider : IProjectTagProvider
    {
        public IEnumerable<IProjectTag> GetAllTags(bool liveOnly = false)
        {
            if (!liveOnly)
            {
                return Tag.GetTags("project").Where(x => x.NodeCount > 2).Select(x => new ProjectTag()
                {
                    Id = x.Id,
                    Text = x.TagCaption,
                    LiveCount = x.NodeCount
                });
            }

            var tagList = new List<IProjectTag>();

            var nodeListingProvider = new NodeListingProvider();

            var projects = nodeListingProvider.GetAllListings(true);

            foreach (var p in projects)
            {
                foreach (var t in p.Tags)
                    if (tagList.Contains(t))
                    {
                        tagList.Find(delegate (IProjectTag tag)
                        {
                            return tag.Id == t.Id;
                        }).LiveCount++;
                    }
                    else
                    {
                        t.LiveCount++;
                        tagList.Add(t);
                    }
            }


            return tagList;
        }

        public IEnumerable<IProjectTag> GetTagsByProjectId(int projectId)
        {
            return Tag.GetTags(projectId).Select(x => new ProjectTag()
            {
                Id = x.Id,
                Text = x.TagCaption,
                Count = x.NodeCount
            });
        }


        public IProjectTag GetTagById(int id)
        {
            return Tag.GetTags("project").Where(x => x.Id == id).Select(y => new ProjectTag()
            {
                Id = y.Id,
                Text = y.TagCaption,
                Count = y.NodeCount
            }).FirstOrDefault();
        }

        public void SetTags(int projectId, string tags)
        {
            Tag.AddTagsToNode(projectId, tags, "project");
        }
    }
}
