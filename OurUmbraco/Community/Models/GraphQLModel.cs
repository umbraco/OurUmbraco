namespace OurUmbraco.Community.Models
{
    public class GraphQLModel
    {
        public class Repository
        {
            public PullRequests PullRequests { get; set; }
        }

        public class PullRequests
        {
            public Edge[] Edges { get; set; }
            public PageInfo PageInfo { get; set; }
        }

        public class PageInfo
        {
            public string EndCursor { get; set; }
        }

        public class Edge
        {
            public Node Node { get; set; }
        }

        public class Node
        {
            public int Number { get; set; }
            public string Mergeable { get; set; }
        }

    }
}
