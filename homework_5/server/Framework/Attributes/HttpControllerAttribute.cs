using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpControllerAttribute : Attribute
    {
        public string Route { get; init; }

        public HttpControllerAttribute(string route)
        {
            Route = route;
        }
    }
}
