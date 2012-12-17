using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using System.IO;
using umbraco.BusinessLogic;
using System.Web.Caching;

namespace uVersion
{
    public class config
    {

        public static XmlDocument _Settings
        {
            get
            {
                XmlDocument us = (XmlDocument)HttpRuntime.Cache["uWikiFileVersionSettingsFile"];
                if (us == null)
                    us = ensureSettingsDocument();
                return us;
            }
        }

        private static string _path = umbraco.GlobalSettings.FullpathToRoot + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar;
        private static string _filename = "uVersion.config";
        private static XmlDocument ensureSettingsDocument()
        {
            object settingsFile = HttpRuntime.Cache["uWikiFileVersionSettingsFile"];

            // Check for language file in cache
            if (settingsFile == null)
            {
                XmlDocument temp = new XmlDocument();
                XmlTextReader settingsReader = new XmlTextReader(_path + _filename);
                try
                {
                    temp.Load(settingsReader);
                    HttpRuntime.Cache.Insert("uWikiFileVersionSettingsFile", temp, new CacheDependency(_path + _filename));
                }
                catch (Exception e)
                {
                    Log.Add(LogTypes.Error, new User(0), -1, "Error reading uWikiFileVersion setting file: " + e.ToString());
                }
                settingsReader.Close();
                return temp;
            }
            else
                return (XmlDocument)settingsFile;
        }

        private static void save()
        {
            _Settings.Save(_path + _filename);
        }

        /// <summary>
        /// Selects a xml node in the umbraco settings config file.
        /// </summary>
        /// <param name="Key">The xpath query to the specific node.</param>
        /// <returns>If found, it returns the specific configuration xml node.</returns>
        public static XmlNode GetKeyAsNode(string Key)
        {
            if (Key == null)
                throw new ArgumentException("Key cannot be null");
            ensureSettingsDocument();
            if (_Settings == null || _Settings.DocumentElement == null)
                return null;
            return _Settings.DocumentElement.SelectSingleNode(Key);
        }

        /// <summary>
        /// Gets the value of configuration xml node with the specified key.
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <returns></returns>
        public static string GetKey(string Key)
        {
            ensureSettingsDocument();

            XmlNode node = _Settings.DocumentElement.SelectSingleNode(Key);
            if (node == null || node.FirstChild == null || node.FirstChild.Value == null)
                return string.Empty;
            return node.FirstChild.Value;
        }



    }
}
