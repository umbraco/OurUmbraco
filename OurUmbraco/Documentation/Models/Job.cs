namespace OurUmbraco.Documentation.Models
{
    public class Job
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool passed { get; set; }
        public bool failed { get; set; }
        public string status { get; set; }
        public string started { get; set; }
        public string finished { get; set; }
        public string duration { get; set; }
        public object[] messages { get; set; }
        public Compilationmessage[] compilationMessages { get; set; }
        public Artifact[] artifacts { get; set; }
    }
}