using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Provider
{
    public class CreateProviderResponse
    {
        public Guid Id { get; set; }

        public string ProviderName { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
