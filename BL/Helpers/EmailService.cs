using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Helpers;

public class EmailService
{
    public async Task SendAsync(string to, string subject, string body)
    {
        var mail = new MailMessage();
        mail.From = new MailAddress("system@company.com");
        mail.To.Add(to);
        mail.Subject = subject;
        mail.Body = body;

        var smtp = new SmtpClient("smtp.gmail.com", 587);
        smtp.Credentials = new NetworkCredential(
            "myrm07996@gmail.com",
            "APP_PASSWORD");
        smtp.EnableSsl = true;

        await smtp.SendMailAsync(mail);
    }
}
