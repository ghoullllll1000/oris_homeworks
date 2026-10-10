using Server.Framework.Handlers;
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
            Handler staticFileHandler = new StaticFileHandler();
            Handler controllerHandler = new ControllerHandler();
            staticFileHandler.Successor = controllerHandler;

            Console.WriteLine("Получен запрос.");

            await staticFileHandler.HandleRequest(context);

            Console.WriteLine($"Запрос обработан: {context.Request.Url}");
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