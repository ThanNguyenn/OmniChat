using Amazon.Runtime.Identity;
using Amazon.Runtime.Internal;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.FeedBack;
using OmniChat.Infrastructure.Dtos.Responses.FeedBack;
using OmniChat.Infrastructure.Dtos.Responses.TaskAction;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class FeedBackService : BaseService<FeedBackService>, IFeedBackService
    {
        public FeedBackService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<FeedBackService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }


        public async Task<PagingResponse<FeedBackResponse>> GetFeedBackByStaffIdAsync(
         Guid staffId,
         int pageIndex = 1,
         int pageSize = 10)
        {
            var feedbackRepo = _unitOfWork.GetRepository<FeedBack>();

            var query = feedbackRepo.GetQueryable()
                .Where(f => f.StaffId == staffId)          
                .OrderByDescending(f => f.Rating);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<FeedBackResponse>>(items);
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PagingResponse<FeedBackResponse>
            {
                Items = mapped,
                Meta = new PaginationMeta
                {
                    CurrentPage = pageIndex,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };
        }

        public async Task<FeedBackResponse> GetFeedBackByIdAsync(Guid feedBackId)
        {
            var feedbackRepo = _unitOfWork.GetRepository<FeedBack>();
            var feedback = await feedbackRepo.GetByIdAsync(feedBackId);

            if (feedback == null)
            {
                return null;
            }

            return _mapper.Map<FeedBackResponse>(feedback);
        }

        public async Task<bool> ErichFeedBackFormAsync(
    Guid conversationId,
    FeedBackRequest feedBackRequest,
    string formUrl)
        {
          
            var conversationRepo = _unitOfWork.GetRepository<SupportConversation>();
            var conversation = await conversationRepo.GetByIdAsync(conversationId);

            if (conversation is null)
                throw new KeyNotFoundException($"Không tìm thấy cuộc hội thoại với Id '{conversationId}'");

            if (conversation.ActiveStaffId is null)
                throw new InvalidOperationException("Cuộc hội thoại này chưa có nhân viên phụ trách.");

        
            var feedbackRepo = _unitOfWork.GetRepository<FeedBack>();
            var feedback = _mapper.Map<FeedBack>(feedBackRequest);

            feedback.StaffId = conversation.ActiveStaffId.Value;
            feedback.SupportConversationId = conversationId;
            feedback.FormUrl = formUrl;

            await feedbackRepo.InsertAsync(feedback);
            await _unitOfWork.CommitAsync();
            return true;
        }

    }
}
