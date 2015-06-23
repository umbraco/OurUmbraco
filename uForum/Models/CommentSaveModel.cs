using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace uForum.Models
{
    [DataContract( Name="comment")]
    public class CommentSaveModel
    {
        [DataMember( Name = "body")]
        public string Body { get; set; }

        [DataMember(Name = "topic")]
        public int Topic { get; set; }

        [DataMember(Name = "parent")]
        public int Parent { get; set; }

    }
}
