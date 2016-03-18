using System;
using System.Timers;
using System.Xml;

namespace OurUmbraco.NotificationsCore
{
    public class SheduledNotification
    {
        public string Name { get; set; }
        public double Interval { get; set; }
        public string Assembly { get; set; }
        public string Type { get; set; }
        public XmlNode Details { get; set; }

        readonly Timer _timer = new Timer();

        public SheduledNotification(string name, double interval, string assembly, string type, XmlNode details)
        {
            Name = name;
            Interval = interval;
            Assembly = assembly;
            Type = type;
            Details = details;
        }
        public void Start()
        {
            _timer.Elapsed += timer_Elapsed;
            _timer.Interval = Interval;
            _timer.Enabled = true;

        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {         
            var assemblyFile = string.Format("{0}.dll", Assembly);

            var nAssembly = System.Reflection.Assembly.LoadFrom(assemblyFile);

            var n = (Notification)Activator.CreateInstance(nAssembly.GetType(Type));

            n.SendNotification(Details);
        }
    }
}
