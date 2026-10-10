using Server.Framework.Attributes;
using System.Net;
using System.Reflection;

namespace Server.Framework.Handlers
{
    internal class ControllerHandler : Handler
    {
        public override async Task HandleRequest(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;
            HttpListenerRequest request = context.Request;

            string path = request.Url!.LocalPath;

            if (true) // TODO:
            {
                //string[] strParams = context.Request.Url!
                //                     .Segments
                //                     .Skip(2)
                //                     .Select(s => s.Replace("/", ""))
                //                     .ToArray();

                //var assembly = Assembly.GetExecutingAssembly();

                //var controller = assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(HttpControllerAttribute))).FirstOrDefault(c => c.Name.ToLower() == controllerName.ToLower());

                //if (controller == null) throw new Exception(); // TODO: 

                //var test = typeof(HttpControllerAttribute).Name;
                //var method = controller.GetMethods().Where(t => t.GetCustomAttributes(true)
                //.Any(attr => attr.GetType().Name.Equals(context.Request.HttpMethod, StringComparison.OrdinalIgnoreCase)))
                //.FirstOrDefault();

                //if (method == null) throw new Exception(); // TODO: 

                //object[] queryParams = method.GetParameters()
                //.Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                //.ToArray();

                //var ret = method.Invoke(Activator.CreateInstance(controller), queryParams);
            }
            else if (Successor != null)
            {
                await Successor.HandleRequest(context);
            }
        }
    }
}
