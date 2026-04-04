using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface ICreditNoteService
{
    Task<bool> CreateCreditNoteAdjustmentAsync(Guid orderId, double amount);

    Task<bool> CreateCreditNoteRefundAsync(Guid orderId, double amount);
}
