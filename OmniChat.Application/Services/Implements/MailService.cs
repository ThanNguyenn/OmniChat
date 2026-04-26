using Microsoft.Extensions.Options;
using MimeKit;
using OmniChat.Application.MailMap;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task<bool> SendEmailAsync(MailContent mailContent)
        {
            var messsage = new MimeMessage();

            messsage.Sender = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail);

            messsage.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
            messsage.To.Add(new MailboxAddress(mailContent.To, mailContent.To));
            messsage.Subject = mailContent.Subject;

            var builder = new BodyBuilder();

            builder.HtmlBody = mailContent.Body;
            messsage.Body = builder.ToMessageBody();

            var smtp = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
                await smtp.SendAsync(messsage);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            smtp.Disconnect(true);
            return true;
        }
    }
}
