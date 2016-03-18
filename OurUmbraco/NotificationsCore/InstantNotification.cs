using System;
using System.Reflection;
using System.Xml;

namespace OurUmbraco.NotificationsCore
{
    public class InstantNotification
    {
        private delegate void ExecuteDelegate(string config, string assemblydir, string name, params object[] args);
        private readonly ExecuteDelegate _ed;

        public InstantNotification()
        {
            _ed = Execute;
        }

        public void Invoke(string config, string assemblydir, string name, params object[] args)
        {
            _ed.BeginInvoke(config, assemblydir, name, args, null, null);
        }

        public void Execute(string config, string assemblydir, string name, params object[] args)
        {
            try
            {
                var notifications = new XmlDocument();
                notifications.Load(config);

                var settings = notifications.SelectSingleNode("//global");

                var node = notifications.SelectSingleNode(string.Format("//instant//notification [@name = '{0}']", name));

                var details = new XmlDocument();
                var cont = details.CreateElement("details");

                cont.AppendChild(details.ImportNode(settings, true));
                cont.AppendChild(details.ImportNode(node, true));

                details.AppendChild(cont);

                if (node != null)
                {
                    var assemblyFile = string.Format("{0}{1}.dll", assemblydir, node.Attributes["assembly"].Value);

                    var nAssembly = Assembly.LoadFrom(assemblyFile);

                    var n = (Notification)Activator.CreateInstance(nAssembly.GetType(node.Attributes["type"].Value));

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