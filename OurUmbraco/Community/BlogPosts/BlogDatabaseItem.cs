using System;
using Newtonsoft.Json;
using Skybrud.Essentials.Json;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace OurUmbraco.Community.BlogPosts
{

    [TableName(TableName)]
    [PrimaryKey(PrimaryKey, autoIncrement = false)]
    [ExplicitColumns]
    public class BlogDatabaseItem
    {

        public const string TableName = "CommunityBlogItems";

        public const string PrimaryKey = "Id";

        private BlogRssItem _data;

        [Column(PrimaryKey)]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public string Id { get; set; }

        [Column("BlogId")]
        public string BlogId { get; set; }

        [Column("PubDate")]
        public DateTime PublishedDate { get; set; }

        [Column("Title")]
        public string Title { get; set; }

        [Column("Data")]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        public string DataRaw { get; set; }

        [Ignore]
        public BlogRssItem Data
        {
            get { return _data ?? JsonUtils.ParseJsonObject<BlogRssItem>(DataRaw); }
            set { _data = value; DataRaw = JsonConvert.SerializeObject(_data, Formatting.None); }
        }

    }

}