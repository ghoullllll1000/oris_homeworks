using System.Net;
using System.Reflection;
using System.Web;
using Server.Framework.Attributes;
using Server.Framework.Core;
using Server.Framework.HttpResponses;

namespace Server.Framework.Handlers;

internal sealed class ControllerHandler : Handler
{
    public override async Task HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        string[] pathParts = request.Url!.LocalPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (pathParts.Length == 0)
        {
            if (Successor is not null)
                await Successor.HandleRequest(context);
            else
                await SendError(response, 404, "Маршрут не найден.");
            return;
        }

        string controllerName = pathParts[0];
        string methodName = pathParts.Length > 1 ? pathParts[1] : string.Empty;
        Type? controllerType = Assembly.GetExecutingAssembly().GetTypes()
            .FirstOrDefault(type => type.GetCustomAttribute<HttpControllerAttribute>()?.Route
                .Equals(controllerName, StringComparison.OrdinalIgnoreCase) == true);

        if (controllerType is null)
        {
            if (Successor is not null)
                await Successor.HandleRequest(context);
            else
                await SendError(response, 404, "Маршрут не найден.");
            return;
        }

        MethodInfo? targetMethod = controllerType.GetMethods()
            .FirstOrDefault(method => MatchesRoute(method, methodName, request.HttpMethod));

        if (targetMethod is null)
        {
            await SendError(response, 404, "Маршрут не найден.");
            return;
        }

        try
        {
            var formData = new System.Collections.Specialized.NameValueCollection();
            if ((request.HttpMethod == "POST" || request.HttpMethod == "PUT") && request.HasEntityBody)
            {
                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                string body = await reader.ReadToEndAsync();
                formData.Add(HttpUtility.ParseQueryString(body));
            }

            ParameterInfo[] parameters = targetMethod.GetParameters();
            object?[] args = new object?[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                string name = parameter.Name!;
                string? value = parameter.IsDefined(typeof(FormDataAttribute))
                    ? formData[name]
                    : request.QueryString[name];

                if (value is null && !parameter.ParameterType.IsValueType &&
                    Nullable.GetUnderlyingType(parameter.ParameterType) is not null)
                {
                    args[i] = null;
                    continue;
                }

                if (value is null)
                {
                    if (parameter.HasDefaultValue)
                    {
                        args[i] = parameter.DefaultValue;
                        continue;
                    }
                    if (parameter.ParameterType == typeof(string))
                    {
                        args[i] = string.Empty;
                        continue;
                    }
                    throw new FormatException($"Не передан параметр {name}.");
                }

                Type targetType = Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType;
                args[i] = targetType == typeof(string)
                    ? value
                    : Convert.ChangeType(value, targetType);
            }

            var controller = (ControllerBase)Activator.CreateInstance(controllerType)!;
            controller.SetContext(context);
            object? result = targetMethod.Invoke(controller, args);

            if (result is Task task)
            {
                await task;
                result = task.GetType().GetProperty("Result")?.GetValue(task);
            }

            if (result is IHttpResponseTypeResult responseResult)
            {
                byte[] bytes = responseResult.Execute(context);
                if (context.Response.StatusCode != 204 && context.Response.StatusCode != 304)
                {
                    context.Response.ContentLength64 = bytes.Length;
                    if (bytes.Length > 0)
                        await context.Response.OutputStream.WriteAsync(bytes);
                }
            }
            else if (result is string html)
            {
                await HttpResponseWriter.WriteHtmlAsync(response, html);
            }
            else if (result is not null)
            {
                await HttpResponseWriter.WriteAsync(response,
                    System.Text.Json.JsonSerializer.Serialize(result), "application/json; charset=utf-8");
            }

            response.Close();
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            await SendError(response, 400, exception.InnerException.Message);
        }
        catch (FormatException exception)
        {
            await SendError(response, 400, exception.Message);
        }
        catch (Exception exception)
        {
            await SendError(response, 400, exception.Message);
        }
    }

    private static bool MatchesRoute(MethodInfo method, string route, string httpMethod)
    {
        if (httpMethod == "GET")
            return method.GetCustomAttribute<GetAttribute>()?.Route.Equals(route, StringComparison.OrdinalIgnoreCase) == true;
        if (httpMethod == "POST")
            return method.GetCustomAttribute<PostAttribute>()?.Route.Equals(route, StringComparison.OrdinalIgnoreCase) == true;
        if (httpMethod == "PUT")
            return method.GetCustomAttribute<PutAttribute>()?.Route.Equals(route, StringComparison.OrdinalIgnoreCase) == true;
        return false;
    }

    private static async Task SendError(HttpListenerResponse response, int statusCode, string message)
    {
        if (response.OutputStream.CanWrite)
        {
            response.StatusCode = statusCode;
            response.ContentType = "text/plain; charset=utf-8";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes);
        }
        response.Close();
    }
}
