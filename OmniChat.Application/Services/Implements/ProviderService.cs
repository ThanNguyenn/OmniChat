using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Provider;
using OmniChat.Infrastructure.Dtos.Responses.Provider;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class ProviderService : BaseService<ProviderService>, IProviderService
    {
        public ProviderService(IUnitOfWork<OmniChatDbContext> unitOfWork,
            ILogger<ProviderService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
            : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<CreateProviderResponse> CreateProviderAsync(CreateProviderRequest CreateProviderRequest)
        {
            try
            {
                return await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    // Map request 
                    var newProvider = _mapper.Map<Provider>(CreateProviderRequest);

                    // Add into repo
                    await _unitOfWork.GetRepository<Provider>().InsertAsync(newProvider);

                    // return 
                    return _mapper.Map<CreateProviderResponse>(newProvider);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating a provider :{Message}.", ex.Message);
                throw;
            }
        }

        public async Task<PagingResponse<GetAllProviderResponse>> GetAllProviderAsync(int pageNumber = 1, int pageSize = 20, string? providerName = null)
        {
            try
            {
                return await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    // call repo
                    var repo = _unitOfWork.GetRepository<Provider>();

                    // query
                    return await repo.GetPagingListAsync(selector: x => new GetAllProviderResponse
                    {
                        Id = x.Id,
                        ProviderName = x.ProviderName,
                        CreateDate = x.CreateDate,
                    },
                    predicate: string.IsNullOrWhiteSpace(providerName) ? null : x => x.ProviderName.Contains(providerName),
                    orderBy: q => q.OrderByDescending(x => x.CreateDate),
                    page: pageNumber,
                    size: pageSize
                    );
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Get All  provider :{Message}.", ex.Message);
                throw;
            }
        }

        public async Task<Provider> GetProviderAsync(string providerName)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<Provider>();

                return  await repo.SingleOrDefaultAsync(predicate: x => x.ProviderName == providerName);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Get provider  :{Message}.", ex.Message);
                throw;
            }
        }
    }
}
