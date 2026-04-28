using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Provider;
using OmniChat.Infrastructure.Dtos.Responses.Provider;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using Org.BouncyCastle.Asn1.Ocsp;
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

        public async Task<bool> CreateProviderAsync(CreateProviderRequest CreateProviderRequest)
        {
            if (string.IsNullOrWhiteSpace(CreateProviderRequest.ProviderName))
                throw new BadRequestException("ứng dụng liên kết không được để trống.");
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var existProvider = await GetProviderByNameAsync(CreateProviderRequest.ProviderName);
                if (existProvider != null)
                {
                    // provider early exist
                    throw new BadRequestException("ứng dụng liên kết này đã tồn tại trong hệ thống.");
                }

                // Map request 
                var newProvider = _mapper.Map<Provider>(CreateProviderRequest);

                // Add into repo
                await _unitOfWork.GetRepository<Provider>().InsertAsync(newProvider);

                return true;
            });
        }

        public async Task<PagingResponse<GetAllProviderResponse>> GetAllProviderAsync(int pageNumber = 1, int pageSize = 20, string? providerName = null)
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
                    CreateDate = x.CreateDate ?? DateTime.UtcNow,
                },
                predicate: string.IsNullOrWhiteSpace(providerName) ? null : x => x.ProviderName.Contains(providerName),
                orderBy: q => q.OrderByDescending(x => x.CreateDate),
                page: pageNumber,
                size: pageSize
                );
            });
        }

        public async Task<Provider> GetProviderByNameAsync(string providerName)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<Provider>();

                return await repo.SingleOrDefaultAsync(predicate: x => x.ProviderName == providerName);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Get provider  :{Message}.", ex.Message);
                throw;
            }
        }

        public async Task<Provider> GetProviderByIdAsync(Guid providerId)
        {

            if (providerId == Guid.Empty)
                throw new BadRequestException("Mã ứng dụng liên kết không hợp lệ.");

            var repo = _unitOfWork.GetRepository<Provider>();
            var provider = await repo.SingleOrDefaultAsync(predicate: x => x.Id == providerId);

            if (provider == null)
                throw new NotFoundException("Không tìm thấy ứng dụng liên kết yêu cầu.");

            return provider;
        }

        
    }
}
