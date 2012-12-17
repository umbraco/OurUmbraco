using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Reflection;
using System.Xml;


namespace NotificationsCore
{
    public class NotificationExecuter
    {
        public string Name { get; set; }
        public double Interval { get; set; }
        public string Assembly { get; set; }
        public string Type { get; set; }
        public XmlNode Details { get; set; }

        Timer timer = new Timer();

        public NotificationExecuter(string name, double interval, string assembly, string type, XmlNode details)
        {
            this.Name = name;
            this.Interval = interval;
            this.Assembly = assembly;
            this.Type = type;
            this.Details = details;
        }
        public void Start()
        {
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Interval = Interval;
            timer.Enabled = true;

        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {         
            string assemblyFile =
                    String.Format("{0}.dll", Assembly);

            Assembly nAssembly = System.Reflection.Assembly.LoadFrom(assemblyFile);

            Notification n =
                    (Notification)Activator.CreateInstance(
                    nAssembly.GetType(Type));

            n.SendNotification(Details);

        }
    }
}
