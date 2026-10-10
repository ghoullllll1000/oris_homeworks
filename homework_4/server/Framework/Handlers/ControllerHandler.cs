using Server.Framework.Attributes;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web; 

namespace Server.Framework.Handlers
{
    internal class ControllerHandler : Handler
    {
        public override async Task HandleRequest(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;
            HttpListenerRequest request = context.Request;

            string[] pathParts = request.Url!.LocalPath
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathParts.Length >= 2)
            {
                string controllerName = pathParts[0];
                string methodName = pathParts[1];

                var types = Assembly.GetExecutingAssembly().GetTypes();
                Type? controllerType = null;

                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute<HttpControllerAttribute>();
                    if (attr != null && attr.Route.Equals(controllerName, StringComparison.OrdinalIgnoreCase))
                    {
                        controllerType = type;
                        break;
                    }
                }

                if (controllerType != null)
                {
                    MethodInfo? targetMethod = null;
                    foreach (var method in controllerType.GetMethods())
                    {
                        var getAttr = method.GetCustomAttribute<GetAttribute>();
                        if (getAttr != null && getAttr.Route.Equals(methodName, StringComparison.OrdinalIgnoreCase) && request.HttpMethod == "GET")
                        {
                            targetMethod = method;
                            break;
                        }

                        var postAttr = method.GetCustomAttribute<PostAttribute>();
                        if (postAttr != null && postAttr.Route.Equals(methodName, StringComparison.OrdinalIgnoreCase) && request.HttpMethod == "POST")
                        {
                            targetMethod = method;
                            break;
                        }
                    }

                    if (targetMethod != null)
                    {
                        
                        System.Collections.Specialized.NameValueCollection formData = new();
                        if (request.HttpMethod == "POST" && request.HasEntityBody)
                        {
                            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                            string bodyString = await reader.ReadToEndAsync();

                            var parsedBody = HttpUtility.ParseQueryString(bodyString);
                            formData.Add(parsedBody);
                        }

                        var parameters = targetMethod.GetParameters();
                        object[] args = new object[parameters.Length];

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var param = parameters[i];
                            string paramName = param.Name!;
                            string? rawValue = null;

                            if (param.IsDefined(typeof(FormDataAttribute)))
                            {
                                rawValue = formData[paramName];
                            }
                            else if (param.IsDefined(typeof(QueryDataAttribute)) || request.HttpMethod == "GET")
                            {
                                rawValue = request.QueryString[paramName];
                            }
                            else
                            {
                                rawValue = request.QueryString[paramName] ?? formData[paramName];
                            }

                            if (rawValue != null)
                            {
                                args[i] = Convert.ChangeType(rawValue, param.ParameterType);
                            }
                            else
                            {
                                args[i] = param.ParameterType.IsValueType ? Activator.CreateInstance(param.ParameterType)! : "";
                            }
                        }

                        object controllerInstance = Activator.CreateInstance(controllerType)!;
                        object? result = targetMethod.Invoke(controllerInstance, args);

                        if (result is string fileName)
                        {
                            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "static", fileName);
                            if (File.Exists(filePath))
                            {
                                byte[] bytes = await File.ReadAllBytesAsync(filePath);
                                response.ContentType = "text/html; charset=utf-8";
                                response.ContentLength64 = bytes.Length;
                                await response.OutputStream.WriteAsync(bytes);
                                response.Close();
                                return;
                            }
                        }

                        response.StatusCode = 200;
                        response.Close();
                        return;
                    }
                    else
                    {
                        await SendNotFound(response);
                        return;
                    }
                }
                else
                {
                    await SendNotFound(response);
                    return;
                }
            }

            if (Successor != null)
            {
                await Successor.HandleRequest(context);
            }
            else
            {
                await SendNotFound(response);
                return;
            }
        }

        private async Task SendNotFound(HttpListenerResponse response)
        {
            response.StatusCode = 404;

            string errorFilePath = Path.Combine(Directory.GetCurrentDirectory(), "static", "404.html");

            if (File.Exists(errorFilePath))
            {
                byte[] buffer = await File.ReadAllBytesAsync(errorFilePath);
                response.ContentType = "text/html; charset=utf-8";
                response.ContentLength64 = buffer.Length;
                using Stream output = response.OutputStream;

                await output.WriteAsync(buffer);
                await output.FlushAsync();

                response.Close();
            }

            response.Close();
        }
    }
}