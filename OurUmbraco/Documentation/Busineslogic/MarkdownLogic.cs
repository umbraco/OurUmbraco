using System.IO;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Skybrud.SyntaxHighlighter;
using Skybrud.SyntaxHighlighter.Markdig;


namespace OurUmbraco.Documentation.Busineslogic
{
    public class MarkdownLogic
    {
        private readonly string _filePath;

        public MarkdownLogic(string filePath)
        {
            _filePath = filePath;
            AppendAltLessonLink = false;
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

        public bool AppendAltLessonLink { get; set; }

        public string DoTransformation()
        {
            if (File.Exists(_filePath))
            {
                string text = File.ReadAllText(_filePath);

                var clean = Regex.Replace(text, MarkdownLogic.RegEx, new MatchEvaluator(match => LinkEvaluator(match, PrefixLinks)),
                    RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

                var pipeline = new MarkdownPipelineBuilder()
                    .UseAbbreviations()
                    .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
                    .UseCitations()
                    .UseCustomContainers()
                    .UseDefinitionLists()
                    .UseEmphasisExtras()
                    .UseFigures()
                    .UseFooters()
                    .UseFootnotes()
                    .UseGridTables()
                    .UseMathematics()
                    .UseMediaLinks()
                    .UsePipeTables()
                    .UseYamlFrontMatter()
                    .UseListExtras()
                    .UseTaskLists()
                    .UseDiagrams()
                    .UseAutoLinks()
                    .UseSyntaxHighlighter(out SyntaxHighlighterOptions highligther)
                    .Build();

                highligther.AddAlias("json5", Language.Json);

                var transform = Markdown.ToHtml(clean, pipeline);

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

            //Need to ensure we dont append the image links as they 404 if we add altTemplate
            if (AppendAltLessonLink && rawUrl.StartsWith("images/") == false)
            {
                return mdUrlTag.Replace(rawUrl, string.Format("{0}?altTemplate=Lesson", rawUrl.EnsureNoDotsInUrl()));
            }

            return mdUrlTag.Replace(rawUrl, rawUrl.EnsureNoDotsInUrl());
        }
    }
}
