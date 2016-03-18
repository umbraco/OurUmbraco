using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace NotificationsCore.NotificationTypes
{
    public class SampleNotification: Notification
    {
        public SampleNotification()
        {

        }


        public override bool SendNotification(System.Xml.XmlNode details, params object[] args)
        {
            //int i = 0;

            //while (i < 10)
            //{
            //    HttpWebRequest request = (HttpWebRequest)
            //    WebRequest.Create("http://www.google.com");

            //    // execute the request
            //    HttpWebResponse response = (HttpWebResponse)
            //        request.GetResponse();


            //    i++;
            //}

            
            return true;
        }
    }
}
