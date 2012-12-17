using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace our
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

            string contentType = "image/jpeg";

            if (!File.Exists(path))
            {
                //return default image
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = contentType;
                ctx.Response.WriteFile(ctx.Server.MapPath("~/media/avatar/default.jpg"));
            }
            else
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = contentType;
                ctx.Response.WriteFile(path);
            }



        }
    }
}