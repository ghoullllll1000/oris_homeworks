using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Server.Framework.HttpResponses
{
    public interface IHttpResponseTypeResult
    {
        byte[] Execute(HttpListenerContext context);
    }
}
