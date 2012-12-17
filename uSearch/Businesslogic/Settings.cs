using System;
using System.Xml;
using System.Web;

namespace uSearch.Businesslogic
{
	/// <summary>
	/// Summary description for Settings.
	/// </summary>
	public class Settings
	{

		private static bool _initialized = false;

        private HttpContext _context {get; set; }
        
		// Key to use when splitting paths
		public string PathSplit = "s";

		// Directory where indexed data should be stored
        public string IndexDirectory {
            get { return _context.Server.MapPath(umbraco.GlobalSettings.StorageDirectory + "/umbSearch"); }
        }
        		

        public string _configSourcePath {
            get { return _context.Server.MapPath(umbraco.GlobalSettings.StorageDirectory + "/umbracoSearchConfig.xml"); }
        }

        private XmlDocument _configSource { get; set; }

        public string IndexDataWithAliases { get; private set; }
        public bool ExcludeUmbracoNaviHide { get; private set;}
        public string ExcludeNodeTypes { get; private set; }
        public string ExcludeIds { get; private set; }

        public Settings(HttpContext context) {
            _context = context;
        }

		public Settings()
		{
            _context = HttpContext.Current;
        }

		public XmlDocument Source 
		{
			get 
			{
				if (_configSource == null) 
					Reload();

				return _configSource;
			}
		}

		public void Reload() 
		{
			if (_configSource == null)
				_configSource = new XmlDocument();
			_configSource.Load(_configSourcePath);
		}

		private void initializeSettings() 
		{
			if (!_initialized) 
			{
				XmlDocument n = Source;
                
				// Get config items
				ExcludeIds = umbraco.xmlHelper.GetNodeValue(n.DocumentElement.SelectSingleNode("/indexConfiguration/excludeIds"));
				ExcludeNodeTypes = umbraco.xmlHelper.GetNodeValue(n.DocumentElement.SelectSingleNode("/indexConfiguration/excludeNodeTypes"));
				ExcludeUmbracoNaviHide = bool.Parse(umbraco.xmlHelper.GetNodeValue(n.DocumentElement.SelectSingleNode("/indexConfiguration/excludeUmbracoNaviHide")));
				IndexDataWithAliases = umbraco.xmlHelper.GetNodeValue(n.DocumentElement.SelectSingleNode("/indexConfiguration/indexDataWithAliases"));
				
                _initialized = true;
			}
		}
	}
}
