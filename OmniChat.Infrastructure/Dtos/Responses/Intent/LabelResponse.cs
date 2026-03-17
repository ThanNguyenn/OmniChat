using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Intent;

public class LabelResponse
{
    public string? Label { get; set; }
    public float Confidence { get; set; }
    public float Threshold { get; set; }

    public bool Predicted {  get; set; }
}
