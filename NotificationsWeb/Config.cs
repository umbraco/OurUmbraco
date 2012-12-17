using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace NotificationsWeb
{
    public class Config
    {
        private static string _path = umbraco.GlobalSettings.FullpathToRoot + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar;
        private static string _filename = "Notification.config";

        public static string AssemblyDir
        {
            get
            {
                return umbraco.GlobalSettings.FullpathToRoot + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar;
            }
        }


        public static string ConfigurationFile
        {
            get
            {
                return _path + _filename;
            }
        }
    }
}
