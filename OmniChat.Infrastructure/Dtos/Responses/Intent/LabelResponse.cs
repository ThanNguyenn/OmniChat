using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Intent;

public class LabelResponse
{
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("confidence")]
    public float Confidence { get; set; }

    [JsonPropertyName("threshold")]
    public float Threshold { get; set; }

    [JsonPropertyName("predicted")]
    public bool Predicted { get; set; }
}
