using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Xml;
using System.Configuration;
using System.Threading;
using System.Net;

namespace NotificationMailer
{
    public partial class NotificationService : ServiceBase
    {

        public NotificationService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
           
            System.Timers.Timer timer = new System.Timers.Timer();
          
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Interval = double.Parse(ConfigurationSettings.AppSettings["interval"]);
            timer.Enabled = true;

            timer.Start();


            //XmlDocument notifications = new XmlDocument();
            //notifications.Load(ConfigurationSettings.AppSettings["configFile"]);

            //XmlNode settings = notifications.SelectSingleNode("//global");

            //XmlNodeList xnl = notifications.SelectNodes("//sheduled//notification");
            //foreach (XmlNode node in xnl)
            //{
            //    XmlDocument details = new XmlDocument();
            //    XmlNode cont = details.CreateElement("details");

            //    cont.AppendChild(details.ImportNode(settings, true));
            //    cont.AppendChild(details.ImportNode(node, true));

            //    details.AppendChild(cont);

            //    SheduledNotification n =
            //        new SheduledNotification(
            //            node.Attributes["name"].Value,
            //            double.Parse(node.Attributes["interval"].Value),
            //            node.Attributes["assembly"].Value,
            //            node.Attributes["type"].Value,
            //            details);

            //    Thread nThread = new Thread(new ThreadStart(n.Start));
            //    nThread.Start();

            //}
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            HttpWebRequest request = (HttpWebRequest)
            WebRequest.Create(ConfigurationSettings.AppSettings["url"]);

            HttpWebResponse response = (HttpWebResponse)
            request.GetResponse();

            
        }

        protected override void OnStop()
        {
        }


        //public static string GetNodeValue(XmlNode n)
        //{
        //    if (n == null || n.FirstChild == null)
        //        return string.Empty;
        //    return n.FirstChild.Value;
        //}
    }
}
