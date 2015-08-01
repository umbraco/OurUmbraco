using System;
using System.Collections.Generic;

namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface IKarmaProvider
    {
        int GetProjectKarma(int projectId);
        IEnumerable<IKarma> GetProjectsKarmaList();
        IEnumerable<IKarma> GetProjectsKarmaListByDate(DateTime afterDate);
        int AddKarma(IKarma karma);
        void ClearKarma(int projectId);
    }
}
