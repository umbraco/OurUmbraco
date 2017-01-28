using System;
using System.Collections.Generic;
using OurUmbraco.Our.Models;
using umbraco;
using Umbraco.Core.Persistence;

namespace OurUmbraco.Forum.Models
{
    [TableName("forumTopics")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    public class Topic
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("parentId")]
        public int ParentId { get; set; }
        
        [Column("memberId")]
        public int MemberId { get; set; }
        
        [Column("answer")]
        public int Answer { get; set; }

        [Column("title")]
        public string Title { get; set; }
        
        [Column("urlName")]
        public string UrlName { get; set; }
        
        [Column("body")]
        public string Body { get; set; }
        
        [Column("created")]
        public DateTime Created { get; set; }
        
        [Column("updated")]
        public DateTime Updated { get; set; }

        [Column("isSpam")]
        public bool IsSpam { get; set; }

        [Column("latestReplyAuthor")]
        public int LatestReplyAuthor { get; set; }

        [Column("latestComment")]
        public int LatestComment { get; set; }

        [Column("replies")]
        public int Replies { get; set; }

        [Column("score")]
        public int Score { get; set; }

        [Column("locked")]
        public bool Locked { get; set; }

        [Column("version")]
        public int Version { get; set; }

        public MemberData MemberData { get; set; }

        public List<SimpleMember> Votes { get; set; }

        public string GetUrl()
        {
            var url = library.NiceUrl(this.ParentId);
            return GlobalSettings.UseDirectoryUrls
                ? string.Format("/{0}/{1}-{2}", url.Trim('/'), Id, UrlName)
                : string.Format("/{0}/{1}-{2}.aspx", url.Substring(0, url.LastIndexOf('.')).Trim('/'), Id, UrlName);
        }
    }
}
