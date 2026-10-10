using Server.Framework.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Controllers
{
    [HttpController("auth")]
    internal class AuthController
    {
        [Get("login")]
        public void Login()
        {
            // TODO: return login.html
        }

        [Post("login")]
        public void Login(string login, string password)
        {
            // TODO: вывод в консоль login и password из form(и query)
            Console.WriteLine($"Логин: {login}; Пароль: {password}");
        }
    }
}
