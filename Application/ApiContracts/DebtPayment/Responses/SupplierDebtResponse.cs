namespace Application.ApiContracts.DebtPayment.Responses
{
    public class SupplierDebtResponse
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Phone { get; set; }

        public decimal TotalDebt { get; set; }
    }
}
