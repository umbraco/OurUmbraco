using System;using System.Collections.Generic;
using System.Linq;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.Our;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Web;

namespace OurUmbraco.MarketPlace.Providers
{
    public class KarmaProvider : IKarmaProvider
    {
        public int GetProjectKarma(int projectId)
        {
            var db = UmbracoContext.Current.Application.DatabaseContext.Database;
            var result =  db.ExecuteScalar<int>("SELECT SUM(points) karma FROM powersProject WHERE id = @projectId", new { projectId = projectId });
            return result;
        }

        public IEnumerable<IKarma> GetProjectsKarmaList()
        {
            return GetProjectsKarmaList(null);
        }

        public IEnumerable<IKarma> GetProjectsKarmaList(int[] ids)
        {
            var karmaList = new List<IKarma>();

            string sql;
            if (ids == null || ids.Length == 0)
            {
                sql = "SELECT id as ProjectId, SUM(points) Points FROM powersProject GROUP BY id ORDER BY SUM(points) DESC";
            }
            else
            {
                if (ids.Length > 2000) throw new InvalidOperationException("Cannot query for more than 2000 items");

                sql = string.Format(
                    "SELECT id as ProjectId, SUM(points) Points FROM powersProject WHERE id IN ({0}) GROUP BY id ORDER BY SUM(points) DESC",
                    string.Join(",", ids));
            }
                
            using (var reader = Data.SqlHelper.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    var karma = new Karma
                    {
                        ProjectId = reader.GetInt("ProjectId"),
                        Points = reader.GetInt("Points")
                    };
                    karmaList.Add(karma);
                }
            }

            return karmaList;
        }

        public IEnumerable<IKarma> GetProjectsKarmaList(long pageIndex, long pageSize, out long totalItems)
        {
            var pagedResult = ApplicationContext.Current.DatabaseContext.Database.Page<dynamic>(pageIndex + 1, pageSize,
                "SELECT * FROM (SELECT id as ProjectId, SUM(points) Points FROM powersProject GROUP BY id) tmp ORDER BY Points DESC");

            totalItems = pagedResult.TotalItems;

            return pagedResult.Items.Select(x => new Karma
            {
                ProjectId = x.ProjectId,
                Points = x.Points
            });
        }

        public IEnumerable<IKarma> GetProjectsKarmaListByDate(DateTime afterDate)
        {
            throw new NotImplementedException();
        }

        public int AddKarma(IKarma karma)
        {
            throw new NotImplementedException();
        }

        public void ClearKarma(int projectId)
        {
            throw new NotImplementedException();
        }
    }
}
