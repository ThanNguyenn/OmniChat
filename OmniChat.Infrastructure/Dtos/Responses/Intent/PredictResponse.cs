using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Intent;

public class PredictResponse
{
        public List<string>? Intents { get; set; }
        public List<LabelResponse>? Details { get; set; }
}
