using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BL = umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;
using uWiki.Businesslogic;
namespace our.usercontrols
{
    public partial class FileDownloadProxy : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["id"] != null)
            {

                int fileId = int.Parse(Request["id"]);


                WikiFile wf = new WikiFile(fileId);

                wf.UpdateDownloadCounter(false, Request["release"] != null);


//                HttpCookie cookie = HttpContext.Current.Request.Cookies["ProjectFileDownload" + fileId];
//                if (cookie == null)
//                {
//                    int downloads = 0;
//                    downloads = BL.Application.SqlHelper.ExecuteScalar<int>(
//                        "Select downloads from wikiFiles where id = @id;",
//                        BL.Application.SqlHelper.CreateParameter("@id", fileId));

//                    downloads = downloads + 1;
                    

//                    BL.Application.SqlHelper.ExecuteNonQuery(
//                        "update wikiFiles set downloads = @downloads where id = @id;",
//                         BL.Application.SqlHelper.CreateParameter("@id", fileId),
//                         BL.Application.SqlHelper.CreateParameter("@downloads", downloads));



//                    int _currentMember = 0;
//                    Member m = Member.GetCurrentMember();
//                    if(m != null)
//                        _currentMember = m.Id;

//                    WikiFile wf = new WikiFile(fileId);

//                    if (Request["release"] != null)
//                    {

//                        BL.Application.SqlHelper.ExecuteNonQuery(
//                            @"insert into projectDownload(projectId,memberId,timestamp) 
//                        values((select nodeId from wikiFiles where id = @id) ,@memberId, getdate())",
//                                 BL.Application.SqlHelper.CreateParameter("@id", fileId),
//                                 BL.Application.SqlHelper.CreateParameter("@memberId", _currentMember));
//                    }

//                    cookie = new HttpCookie("ProjectFileDownload" + fileId);
//                    cookie.Expires = DateTime.Now.AddHours(1);
//                    HttpContext.Current.Response.Cookies.Add(cookie);

                    
//                }                
                string path = BL.Application.SqlHelper.ExecuteScalar<string>(
                        "Select path from wikiFiles where id = @id;",
                        BL.Application.SqlHelper.CreateParameter("@id", fileId));

                string file = BL.Application.SqlHelper.ExecuteScalar<string>(
                   "Select name from wikiFiles where id = @id;",
                   BL.Application.SqlHelper.CreateParameter("@id", fileId));

                System.IO.FileInfo fileinfo = new System.IO.FileInfo(Server.MapPath(path));

                string extension = System.IO.Path.GetExtension(Server.MapPath(path));
                string type = "";
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