using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Server.Framework.HttpResponses
{
    public class HtmlResult : IHttpResponseTypeResult
    {
        private readonly object _html;

        public HtmlResult(string html)
        {
            _html = html;
        }

        public byte[] Execute(HttpListenerContext context)
        {
            var json = JsonSerializer.Serialize(_html);

            context.Response.ContentType = "text/html";
            
            byte[] buffer = Encoding.UTF8.GetBytes(json); //TODO на подумать 

            return buffer;
        }
    }
}
