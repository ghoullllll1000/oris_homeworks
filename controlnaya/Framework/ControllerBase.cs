using System.Net;
using Server.Framework.HttpResponses;

namespace Server.Framework;

public class ControllerBase
{
    protected HttpListenerContext Context { get; private set; } = null!;

    internal void SetContext(HttpListenerContext context)
    {
        Context = context;
    }

    protected JsonResult Json(object obj) => new(obj);
    protected HtmlResult Html(string html) => new(html);
}
