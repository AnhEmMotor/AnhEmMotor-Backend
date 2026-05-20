namespace Application.ApiContracts.Supplier.Responses
{
    public class SupplierStatisticsResponse
    {
        public int TotalSuppliers { get; set; }

        public int FinancialSuppliers { get; set; }

        public string? CreditSuppliers { get; set; }
    }
}