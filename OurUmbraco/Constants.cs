using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurUmbraco
{
    public static class Constants
    {
        public static class AppSettings
        {
            /// <summary>
            /// The current major version of documentationto display to the user.
            /// </summary>
            public const string DocumentationCurrentMajorVersion = "Documentation:CurrentMajorVersion";
        }

        public static class MemberGroups
        {

            public const string Contributor = "CoreContrib";

        }

        public static class Forum
        {
            public const int HeartcoreVersionNumber = 9999;
            public const string UmbracoHeadlessName = "Heartcore";
        }
    }
}
