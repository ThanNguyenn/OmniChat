using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.ChatTemplate;
using OmniChat.Infrastructure.Dtos.Responses.ChatTemplate;
using OmniChat.Infrastructure.Exceptions;
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
    public class ChatTemplateService : BaseService<ChatTemplateService>, IChatTemplateService
    {
        public ChatTemplateService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<ChatTemplateService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<bool> CreateChatTemplateAsync(ChatTemplateRequest request)
        {

            ValidateRequest(request);

            var repo = _unitOfWork.GetRepository<ChatTemplate>();
            
            var existingTemplate = await repo.SingleOrDefaultAsync(predicate: t => t.Code == request.Code);

            if (existingTemplate != null)
            {
                throw new BadRequestException($"Chat template with code '{request.Code}' already exists");
            }

            var newTemplate = _mapper.Map<ChatTemplate>(request);

            await repo.InsertAsync(newTemplate);

            await _unitOfWork.CommitAsync();
            return true;
        }


        public async Task<bool> UpdateChatTemplateAsync(Guid id, ChatTemplateRequest request)
        {

            ValidateRequest(request);
            
            var repo = _unitOfWork.GetRepository<ChatTemplate>();
            
            var existingTemplate = await repo.GetByIdAsync(id);
            
            if (existingTemplate == null)
            {
                throw new NotFoundException($"Chat template with id '{id}' not found");
            }
            var duplicateTemplate = await repo.SingleOrDefaultAsync(predicate: t => t.Code == request.Code && t.Id != id);
          
            if (duplicateTemplate != null)
            {
                throw new BadRequestException($"Chat template with code '{request.Code}' already exists");
            }

            _mapper.Map(request, existingTemplate);

            repo.Update(existingTemplate);

            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteChatTemplateAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<ChatTemplate>();
            var existingTemplate = await repo.GetByIdAsync(id);
            if (existingTemplate == null)
            {
                throw new NotFoundException($"Chat template with id '{id}' not found");
            }
            repo.Delete(existingTemplate);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<ChatTemplateResponse> GetChatTemplateByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<ChatTemplate>();
            var existingTemplate = await repo.GetByIdAsync(id);
            if (existingTemplate == null)
            {
                throw new NotFoundException($"Chat template with id '{id}' not found");
            }
            return _mapper.Map<ChatTemplateResponse>(existingTemplate);
        }

        public async Task<PagingResponse<ChatTemplateResponse>> GetAllChatTemplateAsync(
         int pageNumber,
         int pageSize,
         string? search)
        {
            var repo = _unitOfWork.GetRepository<ChatTemplate>();

            return await repo.GetPagingListAsync<ChatTemplateResponse>(
                predicate: t =>
                    string.IsNullOrEmpty(search) ||
                    t.Code.Contains(search) ||
                    t.Content.Contains(search),

               orderBy: q => q.OrderBy(x => x.Code),

                selector: e => _mapper.Map<ChatTemplateResponse>(e),

                page: pageNumber,
                size: pageSize
            );
        }

        private void ValidateRequest(ChatTemplateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Length > 50)
                throw new BadRequestException("Code is required and must not exceed 50 characters");

            if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length > 500)
                throw new BadRequestException("Content is required and must not exceed 500 characters");
        }
    }
}
