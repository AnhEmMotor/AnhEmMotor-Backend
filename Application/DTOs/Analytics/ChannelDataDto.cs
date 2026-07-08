namespace Application.DTOs.Analytics
{
    public class ChannelDataDto
    {
        public string Name { get; set; } = string.Empty;
        public int Visits { get; set; }
        public int Orders { get; set; }
        public decimal Amount { get; set; }
        public decimal ChangePercent { get; set; }
        public decimal ConversionRate { get; set; }
    }
}
