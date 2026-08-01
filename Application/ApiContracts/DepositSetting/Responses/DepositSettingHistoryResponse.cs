namespace Application.ApiContracts.DepositSetting.Responses;

public class DepositSettingHistoryResponse
{
    public Guid Id { get; set; }
    public string OrderType { get; set; } = string.Empty;
    public decimal OrderThreshold { get; set; }
    public int DepositRatio { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
