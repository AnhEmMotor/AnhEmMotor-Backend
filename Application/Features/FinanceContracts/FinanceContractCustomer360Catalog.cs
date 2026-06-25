using Application.ApiContracts.FinanceContract.Responses;

namespace Application.Features.FinanceContracts;

public static class FinanceContractCustomer360Catalog
{
    private static readonly IReadOnlyDictionary<string, Customer360Seed> CustomersByContractNumber =
        new Dictionary<string, Customer360Seed>(StringComparer.OrdinalIgnoreCase)
    {
        ["TG-HDSAISON-2026-001"] =
            new("Nguyen Minh Quan", "079202006001", "12 Nguyen Van Linh, phuong Tan Phong, Quan 7, TP. Ho Chi Minh"),
        ["TG-FECREDIT-2026-002"] =
            new("Tran Thi Ngoc Anh", "031198704215", "88 Le Duc Tho, phuong 6, Go Vap, TP. Ho Chi Minh"),
        ["TG-HOMECREDIT-2026-003"] =
            new("Le Hoang Phuc", "052199305678", "25 Pham Van Dong, phuong Hiep Binh Chanh, Thu Duc, TP. Ho Chi Minh"),
        ["TG-MBANK-2026-004"] = new("Pham Thu Ha", "001198901234", "168 Tran Phu, phuong 4, Quan 5, TP. Ho Chi Minh"),
        ["TG-TPBANK-2026-005"] =
            new("Vo Thanh Dat", "075199812345", "42 Cach Mang Thang Tam, phuong 11, Quan 3, TP. Ho Chi Minh")
    };

    public static Customer360Response? GetCustomer360(string? contractNumber)
    {
        if (string.IsNullOrWhiteSpace(contractNumber) ||
            !CustomersByContractNumber.TryGetValue(contractNumber, out var customer))
        {
            return null;
        }
        return new Customer360Response
        {
            FullName = customer.FullName,
            Cccd = customer.Cccd,
            Address = customer.Address
        };
    }

    public static IReadOnlyCollection<string> FindContractNumbers(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return [];
        }
        return CustomersByContractNumber
            .Where(x => Contains(x.Value.FullName, keyword))
            .Select(x => x.Key)
            .ToArray();
    }

    private static bool Contains(string value, string keyword) => value.Contains(
        keyword.Trim(),
        StringComparison.OrdinalIgnoreCase);

    private sealed record Customer360Seed(string FullName, string Cccd, string Address);
}
