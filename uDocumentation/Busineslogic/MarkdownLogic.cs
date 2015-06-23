using System.IO;
using System.Text.RegularExpressions;
using MarkdownDeep;

namespace uDocumentation.Busineslogic
{
    public class MarkdownLogic
    {
        private readonly string _filePath;

        public MarkdownLogic(string filePath)
        {
            _filePath = filePath;
        }

        public const string VersionSession = "DocumentationVersion";
        public const string BaseUrl = "documentation";
        public const string AlternativeTemplate = "DocumentationSubpage";
        public const string DocumentTypeAlias = "Wiki";
        public const string MarkdownBasePath = "Documentation";
        public const string MarkdownPathKey = "MarkdownPath";
        public const string RegEx = @"\[([^\]]+)\]\(([^)]+)\)";

        public string GetMarkdownBasePathWithVersion
        {
            get { return string.Concat(MarkdownBasePath, "\\"); }
        }

        public bool PrefixLinks { get; set; }

        public string DoTransformation()
        {
            if (File.Exists(_filePath))
            {
                string text = File.ReadAllText(_filePath);

                var clean = Regex.Replace(text, MarkdownLogic.RegEx, new MatchEvaluator(match => LinkEvaluator(match, PrefixLinks)),
                    RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

                Markdown md = new Markdown();
                string transform = md.Transform(clean);
                return transform;
            }

            return "<h2>Not Found</h2>";
        }

        private string LinkEvaluator(Match match, bool prefixLinks)
        {
            string mdUrlTag = match.Groups[0].Value;
            string rawUrl = match.Groups[2].Value;

            //Escpae external URLs
            if (rawUrl.StartsWith("http") || rawUrl.StartsWith("https") || rawUrl.StartsWith("ftp"))
                return mdUrlTag;

            //Escape anchor links
            if (rawUrl.StartsWith("#"))
                return mdUrlTag;

            //Correct internal image links
            if (rawUrl.StartsWith("../images/"))
                return mdUrlTag.Replace("../images/", "images/");

            //Used for main page to correct relative links
            if (prefixLinks)
            {
                var p = "/documentation/";

                if (umbraco.NodeFactory.Node.GetCurrent().Parent.NodeTypeAlias == "Project")
                    p = "documentation/";
               
                string temp = string.Concat(p, rawUrl);
                mdUrlTag = mdUrlTag.Replace(rawUrl, temp);
            }

            if (rawUrl.EndsWith("index.md"))
                mdUrlTag = mdUrlTag.Replace("/index.md", "/");
            else
                mdUrlTag.TrimEnd('/');


            return mdUrlTag.Replace(rawUrl, rawUrl.EnsureNoDotsInUrl());
        }
    }
}