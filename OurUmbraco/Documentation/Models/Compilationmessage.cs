using System;

namespace OurUmbraco.Documentation.Models
{
    public class Compilationmessage
    {
        public string category { get; set; }
        public string message { get; set; }
        public string details { get; set; }
        public string fileName { get; set; }
        public int line { get; set; }
        public int column { get; set; }
        public string projectName { get; set; }
        public string projectFileName { get; set; }
        public DateTime created { get; set; }
    }
}