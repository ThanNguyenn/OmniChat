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

             var leftoverAllocations = src.Allocations.ToList();

             foreach (GetCustomerTransactionResponse resTrans in dest.CustomerTransactions)
             {
                 if (resTrans.TransactionType == TransactionType.AllocateForInvoice)
                 {
                     Allocation allocation = null;


                     var originalTrans = src.Transactions.FirstOrDefault(t => t.Id == resTrans.Id);
                     if (originalTrans?.Allocation != null)
                     {
                         allocation = originalTrans.Allocation;
                     }

                     if (allocation == null)
                     {
                         allocation = leftoverAllocations.FirstOrDefault(a =>
                             a.TransactionId == null &&
                             a.Amount == resTrans.Amount);
                     }

                     if (allocation != null)
                     {
                         resTrans.InvoiceId = allocation.InvoiceId;
                         resTrans.PaymentStatus = allocation.Invoice?.InvoiceStatus;

                         leftoverAllocations.Remove(allocation);
                     }
                 }
             }
         });

            CreateMap<Transaction, GetCustomerTransactionResponse>()
                .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore());
    }
}
