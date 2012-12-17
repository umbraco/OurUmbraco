namespace uDocumentation.Busineslogic
{
    public class DefaultVersion
    {
        private static DefaultVersion _instance;

        private DefaultVersion() { }

        public static DefaultVersion Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DefaultVersion();
                }
                return _instance;
            }
        }

        public string Number
        {
            get
            {
                return "master";//Read default version from web.config
            }
        }
    }
}