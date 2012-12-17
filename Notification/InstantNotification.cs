using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;

namespace NotificationsCore
{
    public class InstantNotification
    {
        private delegate void ExecuteDelegate(string config, string assemblydir, string name, params object[] args);
        private ExecuteDelegate ed;

        public InstantNotification()
        {
            ed = new ExecuteDelegate(this.Execute);
        }

        public void Invoke(string config, string assemblydir, string name, params object[] args)
        {
            ed.BeginInvoke(config, assemblydir, name, args, null, null);
        }

        public void Execute(string config,string assemblydir, string name, params object[] args)
        {
            try
            {
            XmlDocument notifications = new XmlDocument();
            notifications.Load(config);

            XmlNode settings = notifications.SelectSingleNode("//global");

            XmlNode node = notifications.SelectSingleNode(
                string.Format("//instant//notification [@name = '{0}']", name));

            XmlDocument details = new XmlDocument();
            XmlNode cont = details.CreateElement("details");

            cont.AppendChild(details.ImportNode(settings,true));
            cont.AppendChild(details.ImportNode(node,true));

            details.AppendChild(cont);

            if (node != null)
            {


                string assemblyFile =
                       String.Format("{0}{1}.dll", assemblydir, node.Attributes["assembly"].Value);

                Assembly nAssembly = System.Reflection.Assembly.LoadFrom(assemblyFile);

                Notification n =
                        (Notification)Activator.CreateInstance(
                        nAssembly.GetType(node.Attributes["type"].Value));

                n.SendNotification(details, args);
            }
            else
            {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, 666, "not found");
            }

            }
            catch (Exception e)
            {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, 666, e.Message);
            }
        }
    }
}
