using System.Net;
using System.Text;

namespace Server.Framework
{
    public class HttpServer
    {
        private readonly HttpListener server;
        private bool isRunning;


        public HttpServer(Settings settings)
        {
            server = new HttpListener();

            string uriPrefix = $"http://{settings.Server.Host}:{settings.Server.Port}/{settings.Server.Path}";

            server.Prefixes.Add(uriPrefix);
        }

        public async Task Start()
        {
            server.Start();
            isRunning = true;

            Console.WriteLine("Сервер запущен и слушает:");

            foreach (string prefix in server.Prefixes)
            {
                Console.WriteLine(prefix);
            }

            while (isRunning)
            {
                try
                {
                    HttpListenerContext context =
                        await server.GetContextAsync();

                    await HandleRequest(context);
                }
                catch (Exception)
                {
                    break;
                }
            }

            Console.WriteLine("Сервер завершил работу.");
        }

        private async Task HandleRequest(HttpListenerContext context)
        {
            if (!server.IsListening)
            {
                return;
            }

            HttpListenerResponse response = context.Response;
            HttpListenerRequest request = context.Request;

            string requestUrl = request.Url.LocalPath.Trim('/');

            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "static");
            string filePath = Path.GetFullPath(Path.Combine(rootPath, requestUrl));

            if (Directory.Exists(filePath))
            {
                filePath = Path.Combine(filePath, "index.html");
            }

            FileInfo fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists)
            {
                response.StatusCode = 404;
                filePath = Path.Combine(rootPath, "404.html");
            }
            else
            {
                response.StatusCode = 200;
            }

            byte[] buffer = await File.ReadAllBytesAsync(filePath);
            response.ContentType = GetContentType(filePath);
            response.ContentLength64 = buffer.Length;
            using Stream output = response.OutputStream;

            await output.WriteAsync(buffer);
            await output.FlushAsync();

            response.Close();

            Console.WriteLine("Запрос обработан.");
        }

        private string GetContentType(string filePath)
        {
            string result;

            switch (new FileInfo(filePath).Extension)
            {
                case ".html":
                    result = "text/html; charset=utf-8";
                    break;
                case ".htm":
                    result = "text/html; charset=utf-8";
                    break;
                case ".css":
                    result = "text/css; charset=utf-8";
                    break;
                case ".js":
                    result = "text/javascript; charset=utf-8";
                    break;
                case ".png":
                    result = "image/png";
                    break;
                case ".ico":
                    result = "image/x-icon";
                    break;
                case ".svg":
                    result = "image/svg+xml";
                    break;
                case ".jpg":
                    result = "image/jpeg";
                    break;
                case ".jpeg":
                    result = "image/jpeg";
                    break;
                case ".gif":
                    result = "image/gif";
                    break;
                case ".webp":
                    result = "image/webp";
                    break;
                default:
                    result = "text/html; charset=utf-8";
                    break;
            }

            return result;
        }

        public void Stop()
        {
            if (!isRunning)
                return;

            isRunning = false;

            server.Stop();

            Console.WriteLine("Сервер остановлен.");
        }
    }
}