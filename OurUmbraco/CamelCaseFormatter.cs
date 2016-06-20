using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Newtonsoft.Json.Serialization;

namespace OurUmbraco
{
    /// <summary>
    /// Applying this attribute to any webapi controller will ensure that it only contains one json formatter compatible with the angular json vulnerability prevention.
    /// </summary>
    /// <remarks>
    /// This is in Umbraco core as JsonCamelCaseFormatter but it has a bug so we've copied it here for now until 7.5 is out.
    /// </remarks>
    internal class CamelCaseFormatter : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            //remove all json formatters then add our custom one
            var toRemove = controllerSettings.Formatters.Where(t => (t is JsonMediaTypeFormatter)).ToList();
            foreach (var r in toRemove)
            {
                controllerSettings.Formatters.Remove(r);
            }

            var jsonFormatter = new JsonMediaTypeFormatter
            {
                SerializerSettings =
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            };
            controllerSettings.Formatters.Add(jsonFormatter);
        }
    }
}
