using System.Net;
using System.Text;

namespace Server.Framework.HttpResponses;

public sealed class HtmlResult : IHttpResponseTypeResult
{
    private readonly string html;

    public HtmlResult(string html)
    {
        this.html = html;
    }

    public byte[] Execute(HttpListenerContext context)
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        return Encoding.UTF8.GetBytes(html);
    }
}
