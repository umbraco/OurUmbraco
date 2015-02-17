using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;

namespace NotificationsWeb.Services
{
    public class NotificationService: IDisposable
    {
        private DatabaseContext DatabaseContext;

        public NotificationService()
        {
            init(ApplicationContext.Current.DatabaseContext);
        }

        public NotificationService(DatabaseContext dbContext)
        {
            init(dbContext);
        }

        private void init(DatabaseContext dbContext)
        {
            DatabaseContext = dbContext;
        }

        public static NotificationService Instance
        {
            get
            {
                return Singleton<NotificationService>.UniqueInstance;
            }
        }

        public void Dispose()
        {
            DatabaseContext.DisposeIfDisposable();
        }
    }
}
