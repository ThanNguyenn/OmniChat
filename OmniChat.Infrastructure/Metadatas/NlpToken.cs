using Catalyst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Metadatas;

public sealed class NlpToken
{
    public int Index { get; init; }
    public string Value { get; init; } = "";
    public string Lemma { get; init; } = "";
    public PartOfSpeech POS { get; init; }
    public int Sentence { get; init; }
    public bool IsStructurallySuspicious { get; set; }
}

public enum DiscourseRole
{
    Continue,
    Downgrade,
    Override
}