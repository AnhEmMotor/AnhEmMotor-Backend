using Domain.Entities;
using Sieve.Models;

namespace Application.Features.FinanceContracts.Queries.GetFinanceContractsList;

public static class FinanceContractKeywordFilter
{
    private static readonly string[] KeywordPrefixes = ["Keyword@=", "ContractNumber@="];

    public static FinanceContractKeywordFilterResult Apply(IQueryable<FinanceContract> query, SieveModel sieveModel)
    {
        var normalizedModel = Clone(sieveModel);
        if (string.IsNullOrWhiteSpace(sieveModel.Filters))
        {
            return new FinanceContractKeywordFilterResult(query, normalizedModel);
        }
        var remainingFilters = new List<string>();
        var keywords = new List<string>();
        foreach (var term in sieveModel.Filters
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (TryGetKeyword(term, out var keyword))
            {
                keywords.Add(keyword);
                continue;
            }
            remainingFilters.Add(term);
        }
        normalizedModel.Filters = remainingFilters.Count > 0 ? string.Join(',', remainingFilters) : null;
        foreach (var keyword in keywords.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            query = ApplyKeyword(query, keyword);
        }
        return new FinanceContractKeywordFilterResult(query, normalizedModel);
    }

    private static IQueryable<FinanceContract> ApplyKeyword(IQueryable<FinanceContract> query, string keyword)
    {
        var search = keyword.Trim();
        if (search.Length == 0)
        {
            return query;
        }
        var matchingCustomerContractNumbers = FinanceContractCustomer360Catalog
            .FindContractNumbers(search)
            .ToArray();
        return matchingCustomerContractNumbers.Length == 0
            ? query.Where(x => x.ContractNumber.Contains(search))
            : query.Where(
                x => x.ContractNumber.Contains(search) || matchingCustomerContractNumbers.Contains(x.ContractNumber));
    }

    private static bool TryGetKeyword(string filterTerm, out string keyword)
    {
        foreach (var prefix in KeywordPrefixes)
        {
            if (filterTerm.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                keyword = filterTerm[prefix.Length..].Trim();
                return keyword.Length > 0;
            }
        }
        keyword = string.Empty;
        return false;
    }

    private static SieveModel Clone(SieveModel source) => new()
    {
        Filters = source.Filters,
        Sorts = source.Sorts,
        Page = source.Page,
        PageSize = source.PageSize
    };
}

public sealed record FinanceContractKeywordFilterResult(IQueryable<FinanceContract> Query, SieveModel SieveModel);
