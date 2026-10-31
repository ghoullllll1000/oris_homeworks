using System.Net;
using System.Text;

namespace Server.Framework.HttpResponses;

public sealed class ResponseResult : IHttpResponseTypeResult
{
    private readonly string content;
    private readonly int statusCode;
    private readonly string contentType;
    private readonly string? location;

    public ResponseResult(string content, int statusCode, string contentType = "text/plain; charset=utf-8", string? location = null)
    {
        this.content = content;
        this.statusCode = statusCode;
        this.contentType = contentType;
        this.location = location;
    }

    public byte[] Execute(HttpListenerContext context)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = contentType;
        if (location is not null)
            context.Response.RedirectLocation = location;
        return Encoding.UTF8.GetBytes(content);
    }
}
