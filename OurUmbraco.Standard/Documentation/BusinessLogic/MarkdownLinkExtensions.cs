namespace OurUmbraco.Standard.Documentation.Businesslogic
{
    public static class MarkdownLinkExtensions
    {
        public static string EnsureNoDotsInUrl(this string url)
        {
            return url.Replace(".", "_")
                .Replace("__", "..")
                .Replace("_md", "")
                .Replace("_png", ".png")
                .Replace("_jpg", ".jpg")
                .Replace("_pdf", ".pdf")
                .Replace("_gif", ".gif");
        }

    }
}
