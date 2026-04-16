using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Keyword;

public class CreateKeywordRequest
{
    public Guid IntentTypeId { get; set; }
    public string KeywordText { get; set; }

    public float Weight { get; set; }
}
