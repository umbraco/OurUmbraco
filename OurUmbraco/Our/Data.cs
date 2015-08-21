using umbraco.DataLayer;

namespace OurUmbraco.Our
{
    public class Data
    {
        private static string _ConnString = umbraco.GlobalSettings.DbDSN;
        private static ISqlHelper _sqlHelper;

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        public static ISqlHelper SqlHelper
        {
            get
            {
                if (_sqlHelper == null)
                {
                    try
                    {
                        _sqlHelper = DataLayerHelper.CreateSqlHelper(_ConnString);
                    }
                    catch { }
                }
                return _sqlHelper;
            }
        }
    }
}
