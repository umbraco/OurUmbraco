using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.DataLayer;

namespace NotificationsWeb.BusinessLogic
{
    public class Data
    {
        private static string _ConnString = umbraco.GlobalSettings.DbDSN;
        private static ISqlHelper _sqlHelper;

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
