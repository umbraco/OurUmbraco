using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace uProject.Models
{
    [DataContract(Name = "projectForum", Namespace = "")]
    public class ProjectForum
    {
        [DataMember(Name = "forumId")]
        public int Id { get; set; }

        [DataMember(Name = "parentId",IsRequired=true)]
        [Required]
        public int ParentId { get; set; }

        [DataMember(Name = "title", IsRequired = true)]
        [Required]
        public string Title { get; set; }

        [DataMember(Name = "description", IsRequired = true)]
        [Required]
        public string Description { get; set; }
    }
}