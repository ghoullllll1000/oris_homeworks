using System.Net;
using System.Text;

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
            catch (HttpListenerException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }

        Console.WriteLine("Сервер завершил работу.");
    }

    private async Task HandleRequest(HttpListenerContext context)
    {
        HttpListenerResponse response = context.Response;

        if (!File.Exists("search-engine.html"))
        {
            Console.WriteLine("Ошибка: файл search-engine.html не найден.");

            response.StatusCode = 404;

            string errorText = "Файл search-engine.html не найден.";
            byte[] errorBuffer = Encoding.UTF8.GetBytes(errorText);

            response.ContentLength64 = errorBuffer.Length;

            await response.OutputStream.WriteAsync(errorBuffer);
            response.Close();

            return;
        }

        string htmlFileText =
            await File.ReadAllTextAsync("search-engine.html");

        byte[] buffer =
            Encoding.UTF8.GetBytes(htmlFileText);

        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        response.StatusCode = 200;

        using Stream output = response.OutputStream;

        await output.WriteAsync(buffer);
        await output.FlushAsync();

        response.Close();

        Console.WriteLine("Запрос обработан.");
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
