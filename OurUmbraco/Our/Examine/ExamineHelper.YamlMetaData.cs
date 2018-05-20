using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurUmbraco.Our.Examine
{
    public partial class ExamineHelper
    {
        /// <summary>
        /// Internal class for deserializing the YAML metadata
        /// </summary>
        internal class YamlMetaData
        {
            public string VersionTo { get; internal set; }
            public string Tags { get; internal set; }
            public string VersionFrom { get; internal set; }
        }
    }
}
