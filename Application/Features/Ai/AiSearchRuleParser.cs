using Application.ApiContracts.Ai;
using System.Globalization;
using System.Text;

namespace Application.Features.Ai;

/// <summary>
/// Bóc tách ý định tìm kiếm bằng fuzzy-match trên các từ điển brand/category/vehicleType/màu đã có sẵn trong DB + regex
/// giá tiếng Việt, KHÔNG gọi LLM. Chỉ khi câu nhập còn quá nhiều từ chưa giải thích được (typo nặng, diễn đạt lạ, ý
/// định phức tạp) mới trả về null để AiController fallback sang AI Sidecar.
/// </summary>
public static class AiSearchRuleParser
{
    private const double SimilarityThreshold = 0.75;
    private const int MaxUnmatchedWords = 3;

    private static readonly string[] Colors = ["đỏ", "đen", "trắng", "xanh", "vàng", "bạc", "xám", "cam", "tím", "hồng", "nâu"];

    private static readonly Dictionary<string, long> UnitMultipliers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["trieu"] = 1_000_000,
        ["tr"] = 1_000_000,
        ["nghin"] = 1_000,
        ["k"] = 1_000
    };

    public static AiSearchResult? TryParse(
        string keyword,
        IReadOnlyList<string> brands,
        IReadOnlyList<string> categories,
        IReadOnlyList<string> vehicleTypes)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new AiSearchResult();
        var words = CollapseKnownColloquialisms(
            SplitGluedPriceTokens(keyword.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)));
        var consumed = new bool[words.Length];
        var brand = MatchEntity(words, consumed, brands);
        var vehicleType = MatchEntity(words, consumed, vehicleTypes);
        var category = MatchEntity(words, consumed, categories);
        var (priceMin, priceMax) = MatchPrice(words, consumed);
        var colors = MatchColors(words, consumed);
        var leftoverCount = consumed.Count(c => !c);
        if (leftoverCount > MaxUnmatchedWords)
            return null;
        var result = new AiSearchResult
        {
            Keyword = string.Join(' ', words.Where((_, i) => !consumed[i])),
            Brand = brand ?? string.Empty,
            Category = category ?? string.Empty,
            VehicleType = vehicleType ?? string.Empty,
            Colors = colors,
            Intent = "search"
        };
        if (priceMin.HasValue)
            result.PriceMin = (int)priceMin.Value;
        if (priceMax.HasValue)
            result.PriceMax = (int)priceMax.Value;
        return result;
    }

    private static string[] SplitGluedPriceTokens(string[] words)
    {
        var result = new List<string>(words.Length);
        foreach (var word in words)
        {
            var normalized = Normalize(word);
            var unit = UnitMultipliers.Keys
                .Where(u => normalized.Length > u.Length && normalized.EndsWith(u, StringComparison.Ordinal))
                .OrderByDescending(u => u.Length)
                .FirstOrDefault(u => TryParseNumber(normalized[..^u.Length], out _));
            if (unit == null)
            {
                result.Add(word);
                continue;
            }
            result.Add(normalized[..^unit.Length]);
            result.Add(unit);
        }
        return[.. result];
    }

    private static string[] CollapseKnownColloquialisms(string[] words)
    {
        var normalized = words.Select(Normalize).ToArray();
        var result = new List<string>(words.Length);
        for (var i = 0; i < words.Length; i++)
        {
            if (i + 2 < words.Length && normalized[i] == "xe" && normalized[i + 1] == "tay" && normalized[i + 2] == "ga")
            {
                result.Add(words[i]);
                result.Add(words[i + 2]);
                i += 2;
                continue;
            }
            result.Add(words[i]);
        }
        return[.. result];
    }

    private static string? MatchEntity(string[] words, bool[] consumed, IReadOnlyList<string> dictionary)
    {
        string? best = null;
        var bestScore = 0.0;
        var bestStart = -1;
        var bestLen = 0;
        foreach (var entry in dictionary)
        {
            if (string.IsNullOrWhiteSpace(entry))
                continue;
            var entryWords = entry.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(Normalize).ToArray();
            for (var start = 0; start + entryWords.Length <= words.Length; start++)
            {
                if (Enumerable.Range(start, entryWords.Length).Any(i => consumed[i]))
                    continue;
                var allWordsPass = true;
                var totalScore = 0.0;
                for (var k = 0; k < entryWords.Length && allWordsPass; k++)
                {
                    var wordScore = Similarity(Normalize(words[start + k]), entryWords[k]);
                    var wordThreshold = entryWords[k].Length < 4 ? 0.99 : SimilarityThreshold;
                    allWordsPass = wordScore >= wordThreshold;
                    totalScore += wordScore;
                }
                if (!allWordsPass)
                    continue;
                var score = totalScore / entryWords.Length;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = entry;
                    bestStart = start;
                    bestLen = entryWords.Length;
                }
            }
        }
        if (best == null)
            return null;
        for (var i = bestStart; i < bestStart + bestLen; i++)
            consumed[i] = true;
        return best;
    }

    private static List<string> MatchColors(string[] words, bool[] consumed)
    {
        var found = new List<string>();
        for (var i = 0; i < words.Length; i++)
        {
            if (consumed[i])
                continue;
            var normalizedWord = Normalize(words[i]);
            var match = Colors.FirstOrDefault(c => Similarity(normalizedWord, Normalize(c)) >= SimilarityThreshold);
            if (match == null)
                continue;
            found.Add(match);
            consumed[i] = true;
        }
        return found;
    }

    private static (long? Min, long? Max) MatchPrice(string[] words, bool[] consumed)
    {
        var normalized = words.Select(Normalize).ToArray();
        long? min = null;
        long? max = null;
        for (var i = 0; i < words.Length; i++)
        {
            if (consumed[i])
                continue;
            var token = normalized[i];
            if (token == "tu" && TryMatchRange(words, normalized, consumed, i, out var rangeMin, out var rangeMax))
            {
                min = rangeMin;
                max = rangeMax;
            } else if (token is "duoi" or "tren" && TryMatchBound(normalized, consumed, i, out var boundValue))
            {
                if (token == "duoi")
                    max = boundValue;
                else
                    min = boundValue;
            } else if (TryParseNumber(token, out var number) &&
                TryMatchUnit(normalized, consumed, i + 1, out var multiplier))
            {
                consumed[i] = true;
                consumed[i + 1] = true;
                max = (long)(number * multiplier);
            }
        }
        return (min, max);
    }

    private static bool TryMatchRange(
        string[] words,
        string[] normalized,
        bool[] consumed,
        int tuIndex,
        out long? min,
        out long? max)
    {
        min = null;
        max = null;
        var numIndex1 = tuIndex + 1;
        if (numIndex1 >= words.Length || consumed[numIndex1] || !TryParseNumber(normalized[numIndex1], out var num1))
            return false;
        var cursor = numIndex1 + 1;
        long unit1 = 0;
        var hasUnit1 = cursor < words.Length &&
            !consumed[cursor] &&
            UnitMultipliers.TryGetValue(normalized[cursor], out unit1);
        if (hasUnit1)
            cursor++;
        if (cursor >= words.Length || consumed[cursor] || normalized[cursor] is not ("den" or "toi"))
            return false;
        var denIndex = cursor;
        var numIndex2 = denIndex + 1;
        if (numIndex2 >= words.Length || consumed[numIndex2] || !TryParseNumber(normalized[numIndex2], out var num2))
            return false;
        var unitIndex2 = numIndex2 + 1;
        if (unitIndex2 >= words.Length ||
            consumed[unitIndex2] ||
            !UnitMultipliers.TryGetValue(normalized[unitIndex2], out var unit2))
            return false;
        min = (long)(num1 * (hasUnit1 ? unit1 : unit2));
        max = (long)(num2 * unit2);
        consumed[tuIndex] = true;
        consumed[numIndex1] = true;
        if (hasUnit1)
            consumed[numIndex1 + 1] = true;
        consumed[denIndex] = true;
        consumed[numIndex2] = true;
        consumed[unitIndex2] = true;
        return true;
    }

    private static bool TryMatchBound(string[] normalized, bool[] consumed, int directionIndex, out long value)
    {
        value = 0;
        var numIndex = directionIndex + 1;
        if (numIndex >= normalized.Length || consumed[numIndex] || !TryParseNumber(normalized[numIndex], out var number))
            return false;
        if (!TryMatchUnit(normalized, consumed, numIndex + 1, out var multiplier))
            return false;
        value = (long)(number * multiplier);
        consumed[directionIndex] = true;
        consumed[numIndex] = true;
        consumed[numIndex + 1] = true;
        return true;
    }

    private static bool TryMatchUnit(string[] normalized, bool[] consumed, int unitIndex, out long multiplier)
    {
        multiplier = 0;
        return unitIndex < normalized.Length &&
            !consumed[unitIndex] &&
            UnitMultipliers.TryGetValue(normalized[unitIndex], out multiplier);
    }

    private static bool TryParseNumber(string normalizedToken, out decimal value) => decimal.TryParse(
        normalizedToken.Replace(",", "."),
        NumberStyles.Any,
        CultureInfo.InvariantCulture,
        out value);

    private static string Normalize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        var formD = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd');
    }

    private static double Similarity(string a, string b)
    {
        if (a.Length == 0 && b.Length == 0)
            return 1;
        if (a.Length == 0 || b.Length == 0)
            return 0;
        var distance = LevenshteinDistance(a, b);
        return 1.0 - (double)distance / Math.Max(a.Length, b.Length);
    }

    private static int LevenshteinDistance(string a, string b)
    {
        var dp = new int[a.Length + 1, b.Length + 1];
        for (var i = 0; i <= a.Length; i++)
            dp[i, 0] = i;
        for (var j = 0; j <= b.Length; j++)
            dp[0, j] = j;
        for (var i = 1; i <= a.Length; i++)
        {
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
                if (i > 1 && j > 1 && a[i - 1] == b[j - 2] && a[i - 2] == b[j - 1])
                    dp[i, j] = Math.Min(dp[i, j], dp[i - 2, j - 2] + 1);
            }
        }
        return dp[a.Length, b.Length];
    }
}
