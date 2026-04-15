using AutoMapper;
using OmniChat.Infrastructure.Dtos.Responses.Transaction;
using System.Transactions;

namespace OmniChat.Infrastructure.Mappings;

public class TransactionMapper : Profile
{
    public TransactionMapper()
    {
        CreateMap<Transaction, GetTransactionResponse>();
    }
}
