using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Keyword;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
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
    public class MessageKeywordFilterService : BaseService<MessageKeywordFilterService>, IMessageKeywordFilterService
    {
        public MessageKeywordFilterService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<MessageKeywordFilterService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        private static readonly Regex PhoneRegex =
             new(@"(0|\+84)[0-9]{9}", RegexOptions.Compiled);

        private static readonly Regex EmailRegex =
            new(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);

        private static readonly Regex ProductCodeRegex =
            new(@"\bSP\d{6}\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex OrderCodeRegex =
            new(@"\bOD\d{6}\b",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public async Task<ExtractKeywordResponse> ExtractKeywords(string content, List<string>? productNames = null)
        {
            var result = new ExtractKeywordResponse();

            var productRepo = _unitOfWork.GetRepository<Product>();

            var orderRepo = _unitOfWork.GetRepository<Order>();

            var customerProfileRepo = _unitOfWork.GetRepository<CustomerProfile>();

            // Phone
            var phoneNumbers = PhoneRegex.Matches(content)
                 .Select(m => m.Value)
                 .Distinct()
                 .ToList();

            var customersByPhone = await customerProfileRepo.GetQueryable()
                .Where(cp => phoneNumbers.Contains(cp.PhoneNumber))
                 .ToListAsync();

            foreach (var phone in phoneNumbers)
            {
                result.Highlights.Add(phone);

                var customerInfor = customersByPhone
                    .FirstOrDefault(c => c.PhoneNumber == phone);

                result.Recommends.Add(new IsRecommentOnMesssageResponse
                {
                    Data = customerInfor == null ? null : new SearchCustomerInfoRecommendData
                    {
                        CustomerName = customerInfor.CustomerName,
                        PhoneNumber = customerInfor.PhoneNumber,
                        CustomerAddress = customerInfor.Address
                    },
                    RecommendType = RecommendType.SearchCustomerInfo
                });
            }

            // Email
            var emails = EmailRegex.Matches(content)
                .Select(m => m.Value)
                .Distinct()
                .ToList();

            var customersByEmail = await customerProfileRepo.GetQueryable()
                .Where(cp => emails.Contains(cp.Email))
                 .ToListAsync();

            foreach (var email in emails)
            {
                result.Highlights.Add(email);

                var customerInfor = customersByEmail
                    .FirstOrDefault(c => c.Email == email);

                result.Recommends.Add(new IsRecommentOnMesssageResponse
                {
                    Data = customerInfor == null ? null : new SearchCustomerInfoRecommendData
                    {
                        CustomerName = customerInfor.CustomerName,
                        CustomerEmail = customerInfor.Email,
                        CustomerAddress = customerInfor.Address
                    },
                    RecommendType = RecommendType.SearchCustomerInfo
                });
            }

                // Product Codes
                var productCodes = ProductCodeRegex.Matches(content)
               .Select(m => m.Value.ToUpper())
               .Distinct()
               .ToList();

            // extract Product codes
            if (productCodes.Any())
            {
                var validProducts = await productRepo.GetQueryable()
                    .Where(p => productCodes.Contains(p.Code) && p.IsActive == true)
                    .ToListAsync();

                foreach (var product in validProducts)
                {
                    result.Highlights.Add(product.Code);
                    result.Recommends.Add(new IsRecommentOnMesssageResponse
                    {
                        Data = new SearchProductRecommendData
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            ProductCode = product.Code,
                            ProductImageUrl = product.ImageUrl
                        },

                        RecommendType = RecommendType.SearchProduct
                    });
                }
            }

            // Order codes
            var orderCodes = OrderCodeRegex.Matches(content)
               .Select(m => m.Value.ToUpper())
               .Distinct()
               .ToList();

            // extract Order codes
            if (orderCodes.Any())
            {
                var validOrder = await orderRepo.GetQueryable()
                    .Where(o => orderCodes.Contains(o.Code) && o.IsDeleted == false)
                    .Include(o => o.CustomerProfile)
                    .ToListAsync();
            
                foreach (var order in validOrder)
                {
                    result.Highlights.Add(order.Code);
                    result.Recommends.Add(new IsRecommentOnMesssageResponse
                    {
                        Data = new SearchOrderHistoryRecommendData
                        {
                            OrderId = order.Id,
                            OrderCode = order.Code,
                            TotalAmount = order.TotalAmount,
                            OrderName = order.Name,
                            OrderStatus = order.Status,
                            DeliveryStatus = order.DeliveryStatus,
                            CustomerName = order.CustomerProfile?.CustomerName
                        },
                        RecommendType = RecommendType.SearchOrderHistory
                    });
                }
            }
            return result;
        }
    }
}
