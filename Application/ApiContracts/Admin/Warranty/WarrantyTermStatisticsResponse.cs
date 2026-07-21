namespace Application.ApiContracts.Admin.Warranty;

public class WarrantyTermStatisticsResponse
{
    public int TotalTerms { get; set; }
    public int ActiveTerms { get; set; }
    public int InactiveTerms { get; set; }
    public int BrandsCovered { get; set; }
}
