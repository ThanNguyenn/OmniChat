using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Responses.CustomerProfile;
using OmniChat.Infrastructure.Exceptions;
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
    public  class CustomerMergeService : ICustomerMergeService
    {
        private readonly ICustomerProfileService _customerProfileService;
        private readonly ICustomerMessageService _customerMessageService;
        private readonly ISupportConversationService _supportConversationService;
        private readonly IUnitOfWork<OmniChatDbContext> _unitOfWork;
        private readonly IHubContext<SupportConversationHub> _hubContext;
        private readonly IMapper _mapper;

        public CustomerMergeService(
            ICustomerProfileService customerProfileService,
            ICustomerMessageService customerMessageService,
            ISupportConversationService supportConversationService,
            IUnitOfWork<OmniChatDbContext> unitOfWork,
            IHubContext<SupportConversationHub> hubContext,
            IMapper mapper)
        {
            _customerProfileService = customerProfileService;
            _customerMessageService = customerMessageService;
            _supportConversationService = supportConversationService;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _mapper = mapper;
        }

        public async Task<GetCustomerProfileResponse> MergeAndDeleteAsync(
            Guid sourceId,
            Guid targetId)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var customerRepo = _unitOfWork.GetRepository<CustomerProfile>();

                var source = await _customerProfileService.GetCustomerProfileByIdAsync(sourceId);
                var target = await _customerProfileService.GetCustomerProfileByIdAsync(targetId);

                if (source.Id == target.Id)
                    throw new BusinessException("Cannot merge same customer");

                // Merge senderId
                target.FacebookSenderId ??= source.FacebookSenderId;
                target.ZaloSenderId ??= source.ZaloSenderId;
                target.InstagramSenderId ??= source.InstagramSenderId;

                //Merge Profile Fields
                target.CustomerName = MergeField(target.CustomerName, source.CustomerName);
                target.Email = MergeField(target.Email, source.Email);
                target.PhoneNumber = MergeField(target.PhoneNumber, source.PhoneNumber);
                target.Address = MergeField(target.Address, source.Address);
                target.AvatarUrl = MergeField(target.AvatarUrl, source.AvatarUrl);
                target.IsNewCustomer = false;

                //  Re-assign FK Data 
                await _customerMessageService
                    .UpdateCustomerMessageAfterMergeAsync(source, target);

                await _supportConversationService
                    .UpdateConversationAfterMergeAsync(source, target);

                await customerRepo.DeleteAsync(x => x.Id == source.Id);

                var response = _mapper.Map<GetCustomerProfileResponse>(target);
    
                 customerRepo.Update(target);

                await _hubContext.Clients.All.SendAsync(
                    "SidebarCustomerUpdated",
                    response
                );

                return response;
            });
        }

        private string? MergeField(string? targetValue, string? sourceValue)
        {
            if (string.IsNullOrWhiteSpace(targetValue))
                return sourceValue;

            return targetValue;
        }
    }
}
