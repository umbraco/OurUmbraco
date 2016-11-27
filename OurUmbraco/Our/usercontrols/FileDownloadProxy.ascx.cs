using System;
using OurUmbraco.Wiki.BusinessLogic;
using BL = umbraco.BusinessLogic;

namespace OurUmbraco.Our.usercontrols
{
    public partial class FileDownloadProxy : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["id"] == null)
                return;

            var fileId = int.Parse(Request["id"]);

            var wikiFile = new WikiFile(fileId);

            wikiFile.UpdateDownloadCounter(false, wikiFile.FileType == "package");
            using (var sqlHelper = BL.Application.SqlHelper)
            {
                var path = sqlHelper.ExecuteScalar<string>(
                    "Select path from wikiFiles where id = @id;",
                    sqlHelper.CreateParameter("@id", fileId));

                var file = sqlHelper.ExecuteScalar<string>(
                    "Select name from wikiFiles where id = @id;",
                    sqlHelper.CreateParameter("@id", fileId));

                var fileinfo = new System.IO.FileInfo(Server.MapPath(path));

                var extension = System.IO.Path.GetExtension(Server.MapPath(path));
                var type = "";
                // set known types based on file extension  
                if (extension != null)
                {
                    switch (extension.ToLower())
                    {
                        case ".tif":
                        case ".tiff":
                            type = "image/tiff";
                            break;
                        case ".jpg":
                        case ".jpeg":
                            type = "image/jpeg";
                            break;
                        case ".gif":
                            type = "image/gif";
                            break;
                        case ".docx":
                        case ".doc":
                        case ".rtf":
                            type = "Application/msword";
                            break;
                        case ".pdf":
                            type = "Application/pdf";
                            break;
                        case ".png":
                            type = "image/png";
                            break;
                        case ".bmp":
                            type = "image/bmp";
                            break;
                        default:
                            type = "application/octet-stream";
                            break;
                    }
                }

                Response.Clear();

                Response.AddHeader("Content-Disposition", "attachment; filename= " + MakeSafeFileName(file));
                Response.AddHeader("Content-Length", fileinfo.Length.ToString());
                Response.ContentType = type;
                Response.WriteFile(path);
            }
        }

        private string MakeSafeFileName(string orig)
        {
            return orig.Replace(" ","-");
        }
    }
}