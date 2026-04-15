using AutoMapper;
using OmniChat.Infrastructure.Dtos.Responses.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace OmniChat.Infrastructure.Mappings;

public class TransactionMapper : Profile
{
    public TransactionMapper()
    {
        CreateMap<Transaction, GetTransactionResponse>();
    }
}
