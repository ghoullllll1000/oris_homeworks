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
            message.From.Add(new MailboxAddress("Айгуль Рахматуллина", "studymailghoul@yandex.ru"));
            message.To.Add(new MailboxAddress("Получатель", to));
            message.Subject = "КФУ ИТИС";
            var body = new TextPart("plain") { Text = "Здравствуйте! Дз от Рахматуллиной Айгуль. Группа: 11-541." };

            var builder = new BodyBuilder();
            builder.TextBody = body.GetText(Encoding.UTF8);

            builder.Attachments.Add("server.zip");

            message.Body = builder.ToMessageBody();

            message.MessageId = MimeKit.Utils.MimeUtils.GenerateMessageId();
            message.Date = DateTimeOffset.Now;

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.yandex.ru", 465, SecureSocketOptions.SslOnConnect);
                client.Authenticate("studymailghoul@yandex.ru", "sjweboolojyqfcgo");
                client.Send(message);
                client.Disconnect(true);
            }
            //sjweboolojyqfcgo
            Console.WriteLine("Письмо отправлено!");

        }
    }
}
