using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IPayOsService
    {
        Task<string> CreatePaymentLinkAsync(Guid id);
        Task<bool> HandleWebhookAsync(WebhookType body);
    }
}
