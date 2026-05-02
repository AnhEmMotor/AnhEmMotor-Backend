using System.Globalization;

namespace Infrastructure.Integrations.Payment.VNPay;

public class VnPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (string.Compare(x, y, StringComparison.Ordinal) == 0)
            return 0;
        if (x == null)
            return -1;
        if (y == null)
            return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}
