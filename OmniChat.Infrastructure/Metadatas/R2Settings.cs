using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Metadatas;

public class R2Settings
{
    public string BucketName { get; set; } = default!;
    public string PublicUrl { get; set; } = default!;
    public string AccessKeyId { get; set; } = default!;
    public string SecretAccessKey { get; set; } = default!;
    public string Endpoint { get; set; } = default!;
}

