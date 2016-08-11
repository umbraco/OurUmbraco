using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace OurUmbraco.Our.Examine
{
    /// <summary>
    /// Custom analyzer for projects so that we can have different analyzers per field
    /// </summary>
    public class ProjectAnalyzer : PerFieldAnalyzerWrapper
    {        
        public ProjectAnalyzer()
            : base(new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29))
        {
            AddAnalyzer("projectFolder", new KeywordAnalyzer());
        }

    }
}