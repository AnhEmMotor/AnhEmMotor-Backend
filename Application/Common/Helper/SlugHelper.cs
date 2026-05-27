using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Common.Helper;

public static class SlugHelper
{
    public static string GenerateSlug(string? phrase)
    {
        if (string.IsNullOrEmpty(phrase))
            return string.Empty;
        string str = phrase.ToLowerInvariant();
        str = RemoveDiacritics(str);
        str = Regex.Replace(str, @"[^a-z0-9\s-]", string.Empty);
        str = Regex.Replace(str, @"[\s_-]+", "-").Trim();
        str = str.Substring(0, str.Length <= 255 ? str.Length : 255).Trim('-');
        return str;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharGetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                if (c == 'đ' || c == 'Đ')
                {
                    stringBuilder.Append('d');
                } else
                {
                    stringBuilder.Append(c);
                }
            }
        }
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static UnicodeCategory CharGetUnicodeCategory(char c)
    {
        return CharUnicodeInfo.GetUnicodeCategory(c);
    }
}
