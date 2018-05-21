using System;
using Umbraco.Core.Persistence;

namespace OurUmbraco.HighFiveFeed.Models
{
        [TableName("highFivePosts")]
        [PrimaryKey("id", autoIncrement = false)]
        [ExplicitColumns]
        public class HighFiveFeed
        {
            [Column("id")]
    [TableName("highFivePosts")]
    [PrimaryKey("id", autoIncrement = false)]
    [ExplicitColumns]
    public class HighFiveFeed
    {
        [Column("id")]
        public int Id { get; set; }

            [Column("fromMemberId")]
            public int FromMemberId { get; set; }
        [Column("fromMemberId")]
        public int FromMemberId { get; set; }

            [Column("toMemberId")]
            public int ToMemberId { get; set; }
        [Column("toMemberId")]
        public int ToMemberId { get; set; }

            [Column("actionId")]
            public int ActionId { get; set; }
        [Column("actionId")]
        public int ActionId { get; set; }

            [Column("link")]
            public string Link { get; set; }
        [Column("link")]
        public string Link { get; set; }

            [Column("count")]
            public int Count { get; set; }
        [Column("count")]
        public int Count { get; set; }


        }
        [Column("createdDate")]
        public DateTime CreatedDate { get; set; }
    }
}

