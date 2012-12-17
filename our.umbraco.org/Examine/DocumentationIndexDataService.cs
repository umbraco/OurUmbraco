using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace our.ExamineServices
{
    public class DocumentationIndexDataService : Examine.LuceneEngine.ISimpleDataService
    {
        public IEnumerable<Examine.LuceneEngine.SimpleDataSet> GetAllData(string indexType)
        {
            List<Examine.LuceneEngine.SimpleDataSet> list = new List<Examine.LuceneEngine.SimpleDataSet>();
            return list;        
        }
    }
}