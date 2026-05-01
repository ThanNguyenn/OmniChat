using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.KeywordType;

public class CreateKeywordTypeResquest
{
    [Required(ErrorMessage = "TypeName là bắt buộc.")]
    public string TypeName { get; set; }

    public string Description { get; set; }
}
