using System;

namespace OurUmbraco.Wiki.BusinessLogic
{
    public class WikiHelpRequest
    {
        public string Section { get; set; }
        public string Application { get; set; }
        public string ApplicationPage { get; set; }
        public string Url { get; set; }

        private WikiHelpRequest() { }
        public static WikiHelpRequest Create(string section, string application, string applicationPage, string url)
        {
            try
            {
            WikiHelpRequest w = new WikiHelpRequest();

            w.Section = section;
            w.Application = application;
            w.ApplicationPage = applicationPage;
            w.Url = url;

            w.Save();

            return w;

             } catch (Exception ex) {
                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, ex.ToString());
                
            }

            return null;
        }

        public void Save()
        {
            var helpRequestEventArgs = new HelpRequestEventArgs();

            FireBeforeSave(helpRequestEventArgs);

            if (helpRequestEventArgs.Cancel)
                return;

            using (var sqlHelper = umbraco.BusinessLogic.Application.SqlHelper)
            {
                sqlHelper.ExecuteNonQuery(
                    "INSERT INTO wikiHelpRequest (section, application, applicationPage, url) VALUES(@section, @application, @applicationPage, @url)",
                    sqlHelper.CreateParameter("@section", Section),
                    sqlHelper.CreateParameter("@application", Application),
                    sqlHelper.CreateParameter("@applicationPage", ApplicationPage),
                    sqlHelper.CreateParameter("@url", Url));

                FireAfterSave(helpRequestEventArgs);
            }
        }

        private Events _e = new Events();

        public static event EventHandler<HelpRequestEventArgs> BeforeSave;
        protected virtual void FireBeforeSave(HelpRequestEventArgs e)
        {
            _e.FireCancelableEvent(BeforeSave, this, e);
        }
        public static event EventHandler<HelpRequestEventArgs> AfterSave;
        protected virtual void FireAfterSave(HelpRequestEventArgs e)
        {
            if (AfterSave != null)
                AfterSave(this, e);
        }
    }
}