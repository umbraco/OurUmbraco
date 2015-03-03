using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace our.Examine
{
    /// <summary>
    /// Custom analyzer for projects so that we can have different analyzers per field
    /// </summary>
    public class ProjectAnalyzer : PerFieldAnalyzerWrapper
    {        
        public ProjectAnalyzer()
            : base(new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29))
        {
            //Add custom analyzer for versions field
            AddAnalyzer("versions", new VersionAnalyzer());
            AddAnalyzer("projectFolder", new KeywordAnalyzer());
        }

    }
}