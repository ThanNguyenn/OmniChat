using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IKeywordTypeService
    {
        public  Task<KeywordTypes> GetKeywordTypeByIdAsync(Guid keywordTypeId);

    }
}
