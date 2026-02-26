using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
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

        public async Task<List<string>> ExtractKeywords(string content, List<string>? productNames = null)
        {
            var keywords = new HashSet<string>();

            var productRepo = _unitOfWork.GetRepository<Product>();

            var orderRepo = _unitOfWork.GetRepository<Order>();

            // Phone
            foreach (Match match in PhoneRegex.Matches(content))
                keywords.Add(match.Value);

            // Email
            foreach (Match match in EmailRegex.Matches(content))
                keywords.Add(match.Value);

            // Product Codes
            var productCodes = ProductCodeRegex.Matches(content)
               .Select(m => m.Value.ToUpper())
               .Distinct()
               .ToList();

            // extract Product codes
            if (productCodes.Any())
            {
                var validProductCodes = await productRepo.GetQueryable()
                    .Where(p => productCodes.Contains(p.Code) && p.IsActive == true)
                    .Select(p => p.Code).ToListAsync();

                keywords.UnionWith(validProductCodes);
            }

            // Order codes
            var orderCodes = OrderCodeRegex.Matches(content)
               .Select(m => m.Value.ToUpper())
               .Distinct()
               .ToList();

            // extract Order codes
            if (orderCodes.Any())
            {
                var validOrderCodes = await orderRepo.GetQueryable()
                    .Where(o => orderCodes.Contains(o.Code) && o.IsDeleted == false)
                    .Select(o => o.Code).ToListAsync();
            
                keywords.UnionWith(validOrderCodes);
            }
            return keywords.ToList();
        }
    }
}
