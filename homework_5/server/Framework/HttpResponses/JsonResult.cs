using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Server.Framework.HttpResponses
{
    public class JsonResult : IHttpResponseTypeResult
    {
        private readonly object _data;

        public JsonResult(object data)
        {
            _data = data;
        }

        public byte[] Execute(HttpListenerContext context)
        {
            var json = JsonSerializer.Serialize(_data);

            context.Response.ContentType = "application/json";
            
            byte[] buffer = Encoding.UTF8.GetBytes(json); //TODO на подумать 

            return buffer;
        }
    }
}
