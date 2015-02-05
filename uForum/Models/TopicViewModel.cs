using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace uForum.Models
{
    [DataContract(Name = "topic")]
    public class TopicViewModel
    {
        [DataMember(Name = "body")]
        public string Body { get; set; }

        [DataMember(Name = "forum")]
        public int Forum { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "version")]
        public int Version { get; set; }

    }
}
