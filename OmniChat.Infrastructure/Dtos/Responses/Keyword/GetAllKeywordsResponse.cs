using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Keyword;

public class GetAllKeywordsResponse
{
    public Guid Id { get; set; }

    public string KeywordText { get; set; }

    public Guid IntentTypeId { get; set; }

    public string IntentTypeName { get; set; }

    public float Weight { get; set; }

}


