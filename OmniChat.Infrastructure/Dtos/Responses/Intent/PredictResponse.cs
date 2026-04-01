using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Intent;

public class PredictResponse
{
    [JsonPropertyName("intents")]
    public List<string>? Intents { get; set; }

    [JsonPropertyName("details")]
    public List<LabelResponse>? Details { get; set; }
}
