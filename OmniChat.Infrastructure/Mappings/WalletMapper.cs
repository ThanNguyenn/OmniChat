using AutoMapper;
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
    }
}
