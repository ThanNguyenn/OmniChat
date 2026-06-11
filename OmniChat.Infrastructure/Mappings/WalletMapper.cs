using AutoMapper;
using OmniChat.Infrastructure.Dtos.Responses.Transaction;
using OmniChat.Infrastructure.Dtos.Responses.Wallet;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings;

public class WalletMapper : Profile
{
    public WalletMapper()
    {
        CreateMap<Wallet, GetWalletResponse>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.TotalDebt, opt => opt.Ignore())
            .ForMember(dest => dest.Transactions, opt => opt.MapFrom(src => src.Transactions));

        CreateMap<Transaction, GetTransactionResponse>();

        CreateMap<Wallet, GetCustomerWalletResponse>()
               .ForMember(dest => dest.CustomerTransactions, opt => opt.MapFrom(src => src.Transactions))
               .ForMember(dest => dest.TotalDebt, opt => opt.Ignore())
               .AfterMap((src, dest) =>
               {
                   if (dest.CustomerTransactions == null) return;

                   var allocations = src.Allocations.ToList();
                   foreach (GetCustomerTransactionResponse resTrans in dest.CustomerTransactions)
                   {

                       if (resTrans.TransactionType == TransactionType.AllocateForInvoice)
                       {
                          
                           var allocation = allocations.FirstOrDefault(a => a.Id == resTrans.Id);
                            
                           if(allocation != null)
                           {
                               resTrans.InvoiceId = allocation.InvoiceId;
                               resTrans.PaymentStatus = allocation.Invoice.InvoiceStatus;
                           }
                       }
                   }
               })
               ;

     

        CreateMap<Transaction, GetCustomerTransactionResponse>()
            .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore())
            .ForMember(dest => dest.InvoiceId, opt => opt.Ignore());
    }
}
