using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace OurUmbraco
{
    /// <summary>
    /// Sets the json outgoing/serialized datetime format
    /// </summary>
    internal sealed class OutgoingDateTimeFormatAttribute : Attribute, IControllerConfiguration
    {
        private readonly string _format = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Specify a custom format
        /// </summary>
        /// <param name="format"></param>
        public OutgoingDateTimeFormatAttribute(string format)
        {
            if (string.IsNullOrWhiteSpace(format)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(format));
            _format = format;
        }

        /// <summary>
        /// Will use the standard ISO format
        /// </summary>
        public OutgoingDateTimeFormatAttribute()
        {

        }

        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            var jsonFormatter = controllerSettings.Formatters.OfType<JsonMediaTypeFormatter>();
            foreach (var r in jsonFormatter)
            {
                r.SerializerSettings.Converters.Add(new CustomDateTimeConvertor(_format));
            }
        }


        
    }
}
