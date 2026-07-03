namespace Application.ApiContracts.WarrantyClaim.Responses
{
    public class WarrantyClaimPartResponse
    {
        public int Id { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string PartCode { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public string StatusText { get; set; } = string.Empty;
    }
}
