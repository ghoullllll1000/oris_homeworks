using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Server.Framework.Core
{
    internal static class HttpResponseWriter
    {
        public static async Task WriteHtmlAsync(HttpListenerResponse response, string html, int statusCode = 200)
        {
            await WriteAsync(response, html, "text/html; charset=utf-8", statusCode);
        }

        public static async Task WriteAsync(
            HttpListenerResponse response,
            string text,
            string contextType,
            int statusCode = 200)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            response.StatusCode = statusCode;
            response.ContentType = contextType;
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes);
        }

        public static async Task WriteBytesAsync(HttpListenerResponse response, byte[] bytes, int statusCode = 200)
        {
            response.StatusCode = statusCode;
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes);
        }
    }
}
