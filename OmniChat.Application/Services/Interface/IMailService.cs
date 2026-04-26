using OmniChat.Infrastructure.Dtos.Requests.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IMailService
    {
        Task<bool> SendEmailAsync(MailContent mailContent);
    }
}
