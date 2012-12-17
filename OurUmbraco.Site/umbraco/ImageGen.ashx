<%@ WebHandler Language="c#" Class="RequestHandler" %>

public class RequestHandler : System.Web.IHttpHandler
{
    public bool IsReusable
    {
        get { return false; }
    }

    public void ProcessRequest(System.Web.HttpContext context)
    {
        ImageGen.ImageGenQueryStringParser parser = new ImageGen.ImageGenQueryStringParser();
        parser.Process(context);
        parser = null;
    }
}
