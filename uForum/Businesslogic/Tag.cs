using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;

namespace uForum.Businesslogic
{
    [PetaPoco.TableName("cmsTags")]
    [PetaPoco.PrimaryKey("id")]
    [PetaPoco.ExplicitColumns]
    public class Tag
    {
        [PetaPoco.Column("id")]
        public int Id { get; set; }
        [PetaPoco.Column("parentId")]
        public int? Parent { get; set; }
        [PetaPoco.Column("group")]
        public string Group { get; set; }
        [PetaPoco.Column("tag")]
        public string Name { get; set; }

        public float Weight { get; set; }

        public Tag()
        {

        }

        public Tag(int id, int? parent, string group, string name, float weight)
        {
            Id = id;
            Parent = parent;
            Group = group;
            Name = name;
            Weight = weight;
        }

        public static Tag GetTag(int id)
        {
            var db = new Database("umbracoDbDSN");
            var tag = db.SingleOrDefault<Tag>("where id = @0", id);
            if (tag != null)
                return tag;
            throw new ArgumentNullException(string.Format("No tag found with id = {0}", id));
        }

        public static List<Tag> TagsByTopic(int topicId)
        {
            var tags = new List<Tag>();

            // we use dynamics because it's easier for joins :)
            var db = new Database("umbracoDbDSN");
            foreach (var tagConnection in db.Fetch<dynamic>("select tagId from cmsTagToTopic inner join cmsTags on cmsTags.id = cmsTagToTopic.tagId where topicId = @0", topicId))
            {
                tags.Add(GetTag((int)tagConnection.tagId));
            }

            return tags;
        }

        public static void AddTagsToTopic(int topicId, List<Tag> tags)
        {
            var db = new Database("umbracoDbDSN");

            using (var scope = db.GetTransaction())
            {
                foreach (var tag in tags)
                {
                    addTagToTopic(topicId, tag);
                }
                scope.Complete();
            }
        }

        private static void addTagToTopic(int topicId, Tag tag)
        {
            var db = new Database("umbracoDbDSN");

            if (tag.Id == 0)
            {
                db.Insert(tag);
            }

            var topicTag = db.SingleOrDefault<TopicTag>("WHERE tagId = @0 and topicId = @1", tag.Id, topicId);
            if (topicTag == null)
            {
                topicTag = new TopicTag(tag.Id, topicId, 0);
                db.Insert(topicTag);
            }
        }

        public void AddTagToTopic(int topicId)
        {
            addTagToTopic(topicId, this);
        }

        public IEnumerable<Tag> GetAll()
        {
            var db = new Database("umbracoDbDSN");
            return db.Fetch<Tag>("Select * from cmsTags");

        }
    }

    [PetaPoco.TableName("cmsTagToTopic")]
    [PetaPoco.PrimaryKey("id")]
    [PetaPoco.ExplicitColumns]
    public class TopicTag
    {
        [PetaPoco.Column("id")]
        public int Id { get; set; }
        [PetaPoco.Column("tagId")]
        public int TagId { get; set; }
        [PetaPoco.Column("topicId")]
        public int TopicId { get; set; }
        [PetaPoco.Column("weight")]
        public float Weight { get; set; }

        public TopicTag(int tagId, int topicId, float weight)
        {
            TagId = tagId;
            TopicId = topicId;
            Weight = weight;
        }

        public TopicTag()
        {
        }
    }
}
