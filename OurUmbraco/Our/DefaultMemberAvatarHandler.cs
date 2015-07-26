using System.IO;
using System.Web;

namespace OurUmbraco.Our
{
    public class DefaultMemberAvatarHandler: IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext ctx)
        {
            HttpRequest req = ctx.Request;
            string path = req.PhysicalPath;

            

            if (!File.Exists(path))
            {
                //return default image
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "image/gif";
                ctx.Response.WriteFile(ctx.Server.MapPath("~/media/avatar/default.gif"));
            }
            else
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "image/jpeg";
                ctx.Response.WriteFile(path);
            }



        }
    }
}