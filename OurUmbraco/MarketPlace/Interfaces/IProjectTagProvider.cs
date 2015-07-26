using System.Collections.Generic;

namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface IProjectTagProvider
    {
        IEnumerable<IProjectTag> GetAllTags(bool liveOnly = false);
        IEnumerable<IProjectTag> GetTagsByProjectId(int projectId);
        IProjectTag GetTagById(int id);
        void SetTags(int projectId, string tags);
    }
}
