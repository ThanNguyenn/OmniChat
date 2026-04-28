using AutoMapper;
using Catalyst;
using Catalyst.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mosaik.Core;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Keyword;
using OmniChat.Infrastructure.Dtos.Responses.Intent;
using OmniChat.Infrastructure.Dtos.Responses.Keyword;
using OmniChat.Infrastructure.Dtos.Responses.Order;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements;

public class KeywordService : BaseService<KeywordService>, IKeywordService
{

    private static Pipeline? _nlp;

    public KeywordService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<KeywordService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, Pipeline nlp) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _nlp = nlp;
    }

    public async Task<bool> CreateKeywordAsync(CreateKeywordRequest createKeywordRequest)
    {
        var keywordRepo = _unitOfWork.GetRepository<Keyword>();
        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var keyword = _mapper.Map<Keyword>(createKeywordRequest);
            await keywordRepo.InsertAsync(keyword);
        });
        return true;

    }
    public async Task<bool> DeleteKeywordAsync(Guid keywordId)
    {
        var keywordRepo = _unitOfWork.GetRepository<Keyword>();
        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var existingKeyword = await keywordRepo.SingleOrDefaultAsync(predicate: k => k.Id == keywordId && k.IsDeleted != true) ?? throw new NotFoundException($"Keyword {keywordId} not found");
            existingKeyword.IsDeleted = true;
            keywordRepo.Update(existingKeyword);
            return true;
        });
    }
    public async Task<PagingResponse<GetAllKeywordsResponse>> GetAllKeywordsAsync(
        Guid? intentTypeId,
        string? search,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "id",
        bool descending = true)
    {
        _logger.LogInformation("Fetching keywords with IntentTypeId: {IntentTypeId}, Search: {Search}, PageNumber: {PageNumber}, PageSize: {PageSize}, SortBy: {SortBy}, Descending: {Descending}",
            intentTypeId, search, pageNumber, pageSize, sortBy, descending);
        var keywordRepo = _unitOfWork.GetRepository<Keyword>();
        var response = await keywordRepo.GetPagingListAsync<GetAllKeywordsResponse>(
            predicate: k => k.IsDeleted != true && (intentTypeId == null || k.IntentTypeId == intentTypeId) && (string.IsNullOrEmpty(search) || k.KeywordText.ToLower().Contains(search.ToLower()) || k.IntentType.TypeName.ToLower().Contains(search.ToLower())),
            orderBy: q => OrderBy(q, sortBy, descending),
            include: q => q.Include(k => k.IntentType),
            selector: e => _mapper.Map<GetAllKeywordsResponse>(e),
            page: pageNumber,
            size: pageSize
        );  

        return response;
    }

    private static IOrderedQueryable<Keyword> OrderBy(IQueryable<Keyword> query, string sortBy, bool descending)
    {
        sortBy = sortBy?.Trim().ToLower() ?? "createdate";

        return (sortBy, descending) switch
        {
            ("createdate", false) => query.OrderBy(x => x.CreateDate),
            ("createdate", true) => query.OrderByDescending(x => x.CreateDate),

            ("intenttypeid", false) => query.OrderBy(x => x.IntentTypeId),
            ("intenttypeid", true) => query.OrderByDescending(x => x.IntentTypeId),

            ("intenttypename", false) => query.OrderBy(x => x.IntentType.TypeName),
            ("intenttypename", true) => query.OrderByDescending(x => x.IntentType.TypeName),

            (_, false) => query.OrderBy(x => x.CreateDate),
            (_, true) => query.OrderByDescending(x => x.CreateDate)
        };
    }
    private static IOrderedQueryable<Keyword> ThenOrderBy(
    IOrderedQueryable<Keyword> query,
    string sortBy,
    bool descending)
    {
        sortBy = sortBy?.Trim().ToLower() ?? "intenttypeid";

        Expression<Func<Keyword, object>> keySelector = sortBy switch
        {
            "createdate" => s => s.CreateDate,
            "intenttypeid" => s => s.IntentTypeId,
            "intenttypename" => s => s.IntentType.TypeName,
            _ => s => s.IntentTypeId
        };

        return descending
            ? query.ThenByDescending(keySelector)
            : query.ThenBy(keySelector);
    }


    public async Task<GetKeywordResponse> GetKeywordAsync(Guid keywordId)
    {
        var keywordRepo = _unitOfWork.GetRepository<Keyword>();

        var keyword = await keywordRepo.GetQueryable(predicate: k => k.Id == keywordId && k.IsDeleted != true, asNoTracking: false).FirstOrDefaultAsync();
        return _mapper.Map<GetKeywordResponse>(keyword);
    }
    public Task<bool> UpdateKeywordAsync(Guid keywordId, UpdateKeywordRequest updateKeywordRequest)
    {
        var keywordRepo = _unitOfWork.GetRepository<Keyword>();
        return _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var existingKeyword = await keywordRepo.SingleOrDefaultAsync(predicate: k => k.Id == keywordId && k.IsDeleted != true) ?? throw new NotFoundException($"Không tìm thấy từ khóa");
            _mapper.Map(updateKeywordRequest, existingKeyword);
            keywordRepo.Update(existingKeyword);
            return true;
        });
    }

    //Analyze message with keywords

    public async Task<PredictResponse> AnalyzeMessageWithKeywordsAsync(string message)
    {
        var tokens = await AnalyzeTextAsync(message);
        if (tokens.Count == 0) return new PredictResponse { Intents = new(), Details = new() };

        var keywordRepo = _unitOfWork.GetRepository<Keyword>();
        var allKeywords = await keywordRepo.GetListAsync(
            predicate: k => k.IsDeleted != true,
            include: q => q.Include(k => k.IntentType)
        );

        // Filter nulls and group by Intent Type Name
        var keywordMap = allKeywords
            .Where(k => k.IntentType != null && !string.IsNullOrEmpty(k.KeywordText))
            .GroupBy(k => k.IntentType.TypeName)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(k => k.KeywordText, k => k.Weight, StringComparer.OrdinalIgnoreCase)
            );

        var scores = CalculateScores(tokens, keywordMap);

        const float GlobalThreshold = 5.0f; // Adjust based on your Weight scale

        var details = scores.Select(s => new LabelResponse
        {
            Label = s.Key,
            Confidence = s.Value,
            Threshold = GlobalThreshold,
            Predicted = s.Value >= GlobalThreshold
        })
        .OrderByDescending(d => d.Confidence)
        .ToList();

        return new PredictResponse
        {
            Intents = details.Where(d => d.Predicted).Select(d => d.Label!).ToList(),
            Details = details
        };
    }



    public async Task<IReadOnlyList<NlpToken>> AnalyzeTextAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return Array.Empty<NlpToken>();

        var doc = new Document(message, Language.Vietnamese);
        _nlp.ProcessSingle(doc);

        var results = new List<NlpToken>();
        int globalIndex = 0;
        int sentenceIndex = 0;

        foreach (var sentence in doc)
        {
            foreach (var token in sentence.Tokens)
            {
                results.Add(new NlpToken
                {
                    Index = globalIndex++,
                    Value = token.Value,
                    Lemma = token.Lemma ?? token.Value,
                    POS = token.POS,
                    Sentence = sentenceIndex
                });
            }
            sentenceIndex++;
        }

        MarkStructuralSuspicion(results);
        return results;
    }

    private void MarkStructuralSuspicion(List<NlpToken> tokens)
    {
        foreach (var t in tokens)
        {
            // Vietnamese structural markers often fall into these categories
            if (t.POS is PartOfSpeech.CCONJ or PartOfSpeech.PART or PartOfSpeech.PUNCT)
            {
                t.IsStructurallySuspicious = true;
            }
        }
    }

    private Dictionary<string, float> CalculateScores(
     IReadOnlyList<NlpToken> tokens,
     Dictionary<string, Dictionary<string, float>> keywordMap)
    {
        var scores = keywordMap.Keys.ToDictionary(k => k, _ => 0f);
        var lemmas = tokens.Select(t => t.Lemma).ToArray();
        bool[] consumed = new bool[tokens.Count];

        // Sliding window for Vietnamese compound words
        for (int i = 0; i < tokens.Count; i++)
        {
            if (consumed[i]) continue;

            // 1. Try Trigrams (e.g., "trung tâm đào tạo")
            if (i + 2 < tokens.Count)
            {
                string trigram = $"{lemmas[i]} {lemmas[i + 1]} {lemmas[i + 2]}";
                if (MatchAndScore(trigram, i, 3, keywordMap, scores, tokens, lemmas, consumed)) continue;
            }

            // 2. Try Bigrams (e.g., "bảo hiểm")
            if (i + 1 < tokens.Count)
            {
                string bigram = $"{lemmas[i]} {lemmas[i + 1]}";
                if (MatchAndScore(bigram, i, 2, keywordMap, scores, tokens, lemmas, consumed)) continue;
            }

            // 3. Try Unigrams
            MatchAndScore(lemmas[i], i, 1, keywordMap, scores, tokens, lemmas, consumed);
        }

        return scores;
    }

    private static readonly HashSet<string> ViNegations = new(StringComparer.OrdinalIgnoreCase)
    {
        "không", "chưa", "chẳng", "đừng", "không phải", "chả"
    };

    private bool IsNegated(string[] lemmas, int index, int window = 3)
    {
        int start = Math.Max(0, index - window);
        for (int j = index - 1; j >= start; j--)
        {
            if (ViNegations.Contains(lemmas[j])) return true;
        }
        return false;
    }



    private bool MatchAndScore(string text, int index, int length,
    Dictionary<string, Dictionary<string, float>> keywordMap,
    Dictionary<string, float> scores,
    IReadOnlyList<NlpToken> tokens, string[] lemmas, bool[] consumed)
    {
        bool matched = false;
        foreach (var intent in keywordMap)
        {
            // TryGetValue is case-insensitive if your dictionary was built that way
            if (intent.Value.TryGetValue(text, out var weight))
            {
                float finalWeight = weight;

                // Penalty for suspicious structure (conjunctions/particles)
                if (tokens[index].IsStructurallySuspicious) finalWeight *= 0.6f;

                // Penalty for negation (window of 3 words before the hit)
                if (IsNegated(lemmas, index)) finalWeight *= 0.5f;

                scores[intent.Key] += finalWeight;
                matched = true;
            }
        }

        if (matched)
        {
            for (int j = 0; j < length; j++) consumed[index + j] = true;
        }
        return matched;
    }
    // Include your MatchDepartment and helper logic below...
}
