using System;
using System.Data.SqlClient;
using System.Web;
using Examine;

namespace our
{
    public static class SearchResultExtensions
    {
        public static string GetTitle(this SearchResult result)
        {
            if (result["__IndexType"] == "content")
                return HttpContext.Current.Server.HtmlEncode(result["nodeName"]);
            else if (result["__IndexType"] == "documents" || result["__IndexType"] == "documentation")
                return HttpContext.Current.Server.HtmlEncode(result["Title"]);
            else
                return string.Empty;

        }

        public static string GetDate(this SearchResult result)
        {
            try
            {
                if (result["__IndexType"] == "content")
                    return HttpContext.Current.Server.HtmlEncode(string.Format("Last update: {0}", GetFormattedDateTime(result["updateDate"], "MMMM dd, yyyy")));

                if (result["__IndexType"] == "documents")
                    return HttpContext.Current.Server.HtmlEncode(string.Format("Created: {0} / Last update: {1}", DateTime.Parse(result["Created"]).ToString("MMMM dd, yyyy HH:mm"), DateTime.Parse(result["Updated"]).ToString("MMMM dd, yyyy HH:mm")));

                if (result["__IndexType"] == "documentation")
                    return HttpContext.Current.Server.HtmlEncode(string.Format("Last update: {0}", GetFormattedDateTime(result["dateCreatedSearchAble"], "MMMM dd, yyyy")));
            }
            catch (FormatException ex)
            {
                // Catches "String was not recognized as a valid DateTime." errors
                // TODO: Figure out why these errors occur..
            }

            return string.Empty;
        }

        public static string GetDateSortable(this SearchResult result)
        {
            try
            {
                if (result["__IndexType"] == "content")
                    return GetFormattedDateTime(result["updateDate"], "yyyy-MM-dd HH:mm");

                if (result["__IndexType"] == "documents")
                    return DateTime.Parse(result["Updated"]).ToString("yyyy-MM-dd HH:mm");

                if (result["__IndexType"] == "documentation")
                    return GetFormattedDateTime(result["dateCreatedSearchAble"], "yyyy-MM-dd HH:mm");
            }
            catch (FormatException ex) 
            {
                // Catches "String was not recognized as a valid DateTime." errors
                // TODO: Figure out why these errors occur..
            }
            return string.Empty;
        }

        public static string GetFormattedDateTime(string date, string format)
        {
            try
            {
                var dateTime = new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(date.Substring(6, 2)));
                return dateTime.ToString(format);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string FullUrl(this SearchResult result)
        {
            if (result["__IndexType"] == "content")
                return umbraco.library.NiceUrl(result.Id);

            else if (result["__IndexType"] == "documentation")
            {
                return result["url"];
            }
            else if (result["__IndexType"] == "documents")
                return "TODO";
            //return uForum.Library.NiceTopicUrl(result.Id);
            else
                return string.Empty;
        }

        public static string GenerateBlurb(this SearchResult result, int noOfChars)
        {
            var text = string.Empty;

            if (result["__IndexType"] == "content")
            {
                switch (result.Fields["nodeTypeAlias"])
                {
                    case "WikiPage":
                        //Wiki uses bodyText field
                        if (result.Fields.ContainsKey("bodyText"))
                            text = result.Fields["bodyText"];
                        break;
                    case "Project":
                        //Project uses description field
                        if (result.Fields.ContainsKey("descripiton"))
                            text = result.Fields["description"];
                        break;
                }

            }
            else if (result["__IndexType"] == "documents")
            {

                try
                {
                    if (result.Fields.ContainsKey("Body"))
                        text = result["Body"];
                }
                catch { }
            }
            else if (result["__IndexType"] == "documentation")
            {

                try
                {
                    if (result.Fields.ContainsKey("Body"))
                        text = result["Body"];
                }
                catch { }
            }

            text = umbraco.library.StripHtml(text);

            if (text.Length > noOfChars && text.Length > 0)
            {
                text = text.Substring(0, noOfChars) + "...";
            }

            return text;

        }

        public static string CssClassName(this SearchResult result)
        {
            string cssClass = string.Empty;

            if (result["__IndexType"] == "content")
            {
                switch (result.Fields["nodeTypeAlias"])
                {
                    case "WikiPage":
                        cssClass = " wiki ";
                        break;
                    case "Project":
                        cssClass = " project ";
                        break;
                }
            }
            else if (result["__IndexType"] == "documentation")
            {
                cssClass = " documentation ";
            }
            else if (result["__IndexType"] == "documents")
            {
                cssClass = " forum ";


                SqlConnection cn = new SqlConnection(umbraco.GlobalSettings.DbDSN);
                cn.Open();
                SqlCommand cmd = new SqlCommand("Select answer from forumTopics where id = @id", cn);
                cmd.Parameters.AddWithValue("@id", result.Id);
                int solved = 0;
                try
                {
                    int.TryParse(cmd.ExecuteScalar().ToString(), out solved);
                }
                catch { }
                cn.Close();

                if (solved != 0)
                    cssClass += "solution";
            }


            return cssClass;

        }

        public static string BuildExamineString(this string term, int boost, string field, bool andSearch)
        {
            term = Lucene.Net.QueryParsers.QueryParser.Escape(term);
            var terms = term.Trim().Split(' ');
            var qs = field + ":";
            qs += "\"" + term + "\"^" + (boost + 30000).ToString() + " ";
            qs += field + ":(+" + term.Replace(" ", " +") + ")^" + (boost + 5).ToString() + " ";
            if (!andSearch)
            {
                qs += field + ":(" + term + ")^" + boost.ToString() + " ";
            } return qs;
        }


    }
}