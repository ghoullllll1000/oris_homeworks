using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Server.Framework;
using Server.Framework.Attributes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;


namespace Server.Controllers
{

    [HttpController("home-work")]
    public class Homework : ControllerBase
    {
        [Get("send")]
        public void Send([QueryData] string to)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Отправитель", "studymailghoul@yandex.ru"));
            message.To.Add(new MailboxAddress("Получатель", "studyexample@yandex.ru"));
            message.Subject = "Тестовое письмо из MailKit";
            message.Body = new TextPart("plain") { Text = "Здравствуйте! Дз от Рахматуллиной Айгуль 11-541" };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.yandex.ru", 465, SecureSocketOptions.SslOnConnect);
                client.Authenticate("studymailghoul@yandex.ru", "sjweboolojyqfcgo");
                client.Send(message);
                client.Disconnect(true);
            }

            Console.WriteLine("Письмо отправлено!");

        }
    }
}
