using Server.Framework;
using Server.Framework.Attributes;
using System.Text.Json;
using Server.Framework.HttpResponses;


namespace Server.Controllers
{
    [HttpController("auth")]
    public class AuthController  : ControllerBase
    {
        [Get("login")]
        public string Login()
        {
            return "steam/index.html";
        }

        [Post("login")]
        public string Login([FormData] string login, [FormData] string password)
        {
            Console.WriteLine($"Логин: {login}; Пароль: {password}");
            return "steam/index.html";
        }

        [Get("html")]
        public IHttpResponseTypeResult Html()
        {
            string html = $"""
                <doctype html>
                <html lang="ru"><meta charset="utf-8"><title>Результат формы</title>
                <body>
                </body></html>
                """;

            return Html(html); 
        }

        [Get("json")]
        public IHttpResponseTypeResult Json()
        {
            var json = new { Username = "user", Password = "12345", Age = 13 };

            //var result = JsonSerializer.Serialize(json);

            return Json(json); 
        }

        [Get("json2")]
        public IHttpResponseTypeResult Json2()
        {
            var json = new { Username = "user", Age = 13 };

            //var result = JsonSerializer.Serialize(json);

            return Json(json); 
        }
    }
}