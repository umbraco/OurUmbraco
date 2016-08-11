using System;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Xml;
using umbraco.BusinessLogic;

namespace OurUmbraco.Project.uVersion
{
    public class UVersionConfig
    {
        public static XmlDocument Settings
        {
            get
            {
                XmlDocument us = (XmlDocument)HttpRuntime.Cache["uVersionSettingsFile"];
                if (us == null)
                    us = EnsureSettingsDocument();
                return us;
            }
        }

        private static readonly string Path = umbraco.GlobalSettings.FullpathToRoot + System.IO.Path.DirectorySeparatorChar + "config" + System.IO.Path.DirectorySeparatorChar;
        private static string _filename = "uVersion.config";
        private static XmlDocument EnsureSettingsDocument()
        {
            object settingsFile = HttpRuntime.Cache["uVersionSettingsFile"];

            // Check for language file in cache
            if (settingsFile == null)
            {
                XmlDocument temp = new XmlDocument();
                XmlTextReader settingsReader = new XmlTextReader(Path + _filename);
                try
                {
                    temp.Load(settingsReader);
                    HttpRuntime.Cache.Insert("uVersionSettingsFile", temp, new CacheDependency(Path + _filename));
                }
                catch (Exception e)
                {
                    Log.Add(LogTypes.Error, new User(0), -1, "Error reading uVersion setting file: " + e.ToString());
                }
                settingsReader.Close();
                return temp;
            }
            else
                return (XmlDocument)settingsFile;
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
            EnsureSettingsDocument();
            if (Settings == null || Settings.DocumentElement == null)
                return null;
            return Settings.DocumentElement.SelectSingleNode(Key);
        }

        /// <summary>
        /// Gets the value of configuration xml node with the specified key.
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <returns></returns>
        public static string GetKey(string Key)
        {
            EnsureSettingsDocument();

            XmlNode node = Settings.DocumentElement.SelectSingleNode(Key);
            if (node == null || node.FirstChild == null || node.FirstChild.Value == null)
                return string.Empty;
            return node.FirstChild.Value;
        }



    }
}