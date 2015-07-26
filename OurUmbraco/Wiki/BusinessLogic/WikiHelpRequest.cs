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
            HelpRequestEventArgs e = new HelpRequestEventArgs();

            FireBeforeSave(e);

            if (!e.Cancel)
            {
                Data.SqlHelper.ExecuteNonQuery(
               "INSERT INTO wikiHelpRequest (section, application, applicationPage, url) VALUES(@section, @application, @applicationPage, @url)",
               Data.SqlHelper.CreateParameter("@section", Section),
               Data.SqlHelper.CreateParameter("@application", Application),
                Data.SqlHelper.CreateParameter("@applicationPage", ApplicationPage),
               Data.SqlHelper.CreateParameter("@url", Url));

                FireAfterSave(e);
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