namespace OurUmbraco.NotificationsWeb
{
    public class Config
    {
        private static readonly string Path = umbraco.GlobalSettings.FullpathToRoot + System.IO.Path.DirectorySeparatorChar + "config" + System.IO.Path.DirectorySeparatorChar;
        private static string _filename = "Notification.config";

        public static string AssemblyDir
        {
            get
            {
                return umbraco.GlobalSettings.FullpathToRoot + System.IO.Path.DirectorySeparatorChar + "bin" + System.IO.Path.DirectorySeparatorChar;
            }
        }


        public static string ConfigurationFile
        {
            get
            {
                return Path + _filename;
            }
        }
    }
}
