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
using System.Text.RegularExpressions;
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

            var repo = _unitOfWork.GetRepository<ChatTemplate>();
            
            var existingTemplate = await repo.SingleOrDefaultAsync(predicate: t => t.Code == request.Code);

            if (existingTemplate != null)
            {
                throw new BadRequestException($"Mã Mẫu Chat '{request.Code}' đã tồn tại");
            }

            var newTemplate = _mapper.Map<ChatTemplate>(request);

            await repo.InsertAsync(newTemplate);

            await _unitOfWork.CommitAsync();
            return true;
        }


        public async Task<bool> UpdateChatTemplateAsync(Guid id, ChatTemplateRequest request)
        {
       
            var repo = _unitOfWork.GetRepository<ChatTemplate>();
            
            var existingTemplate = await repo.GetByIdAsync(id);
            
            if (existingTemplate == null)
            {
                throw new NotFoundException($"Không tìm thấy Mẫu Chat");
            }
            var duplicateTemplate = await repo.SingleOrDefaultAsync(predicate: t => t.Code == request.Code && t.Id != id);
          
            if (duplicateTemplate != null)
            {
                throw new BadRequestException($"Mã Mẫu Chat '{request.Code}' đã tồn tại");
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
                throw new NotFoundException($"Không tìm thấy Mẫu Chat");
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
                throw new NotFoundException($"Không tìm thấy Mẫu Chat");
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

        public async Task<string> ExpandTemplateCodesAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return message;

            var repo = _unitOfWork.GetRepository<ChatTemplate>();


            var pattern = @"\b([A-Za-z]{1,5}\d{1,3})\b";
            var matches = Regex.Matches(message, pattern)
                               .Select(m => m.Value)
                               .Distinct()
                               .ToList();

            if (!matches.Any()) return message;

           
            var templates = await repo.GetListAsync(
                predicate: t => matches.Contains(t.Code),
                selector: t => new { t.Code, t.Content }
            );

            var codeMap = templates.ToDictionary(t => t.Code, t => t.Content);

          
            var result = Regex.Replace(message, pattern, match =>
                codeMap.TryGetValue(match.Value, out var content) ? content : match.Value
            );

            return result;
        }
    }
}
