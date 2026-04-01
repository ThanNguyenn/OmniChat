using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Intent;

public class PredictRequest
{
    [JsonPropertyName("text")]
    public string Message { get; set; }
}
