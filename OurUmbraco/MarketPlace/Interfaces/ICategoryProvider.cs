using System.Collections.Generic;

namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface ICategoryProvider
    {
        IEnumerable<ICategory> GetAllCategories();
        ICategory GetCurrent();
        ICategory GetCategory(int id);
    }
}
