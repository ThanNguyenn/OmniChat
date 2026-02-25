using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.KeywordType;

public class UpdateKeywordTypeRequest
{
    public string TypeName { get; set; }

    public string Description { get; set; }
}
