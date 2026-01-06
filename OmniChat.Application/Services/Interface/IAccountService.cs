using OmniChat.Infrastructure.Dtos.Requests.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IAccountService
{
    Task<bool> CreateAccountAsync(CreateAccountRequest createAccountRequest);

}
