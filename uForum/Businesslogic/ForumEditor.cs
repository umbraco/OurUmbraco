using System;
using System.Web;
using uForum.Library;
using umbraco.cms.businesslogic.member;

namespace uForum.Businesslogic
{
    public static class ForumEditor
    {
        public const string EditorChoiceCookieKey = "EditorChoice";
        public const string UseMarkdownEditorPropertyAlias = "useMarkdownEditor";
        public const string EditorChoiceRte = "Rte";
        public const string EditorChoiceMarkdown = "Markdown";

        public static bool UseMarkdownEditor()
        {
            var editorCookie = HttpContext.Current.Request.Cookies[EditorChoiceCookieKey];

            var editorChoice = editorCookie != null
                ? editorCookie[EditorChoiceCookieKey]
                : SetEditorChoiceFromMemberProfile();

            return editorChoice == EditorChoiceMarkdown;
        }

        public static string SetEditorChoiceFromMemberProfile(int id = 0)
        {
            var currentMember = Utills.GetMember(id == 0 ? Member.CurrentMemberId() : id);

            var editorChoice = EditorChoiceRte;

            if (currentMember != null)
            {
                var markdownEditorProperty = currentMember.getProperty(UseMarkdownEditorPropertyAlias);

                if (markdownEditorProperty != null)
                {

                    if (markdownEditorProperty.Value.ToString() == "1")
                    {
                        editorChoice = EditorChoiceMarkdown;
                    }
                }
            }

            SetEditorChoiceCookie(editorChoice);

            return editorChoice;
        }

        public static void SaveEditorChoice(string editorChoice)
        {
            var currentMember = Utills.GetMember(Member.CurrentMemberId());
            var markdownEditorProperty = currentMember.getProperty(UseMarkdownEditorPropertyAlias);

            if (markdownEditorProperty != null)
            {
                var useMarkdownEditor = editorChoice == EditorChoiceMarkdown;
                currentMember.getProperty(UseMarkdownEditorPropertyAlias).Value = useMarkdownEditor ? "1" : "0";
            }

            SetEditorChoiceCookie(editorChoice);
        }

        public static void ClearEditorChoiceCookie()
        {
            HttpContext.Current.Response.Cookies.Remove(EditorChoiceCookieKey);
        }

        private static void SetEditorChoiceCookie(string value)
        {
            var editorChoiceCookie = new HttpCookie(EditorChoiceCookieKey);
            editorChoiceCookie.Values.Add(EditorChoiceCookieKey, value);
            editorChoiceCookie.Expires = DateTime.Now.AddYears(5);
            HttpContext.Current.Response.Cookies.Add(editorChoiceCookie);
        }
    }
}
