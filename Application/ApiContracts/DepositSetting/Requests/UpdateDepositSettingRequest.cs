namespace Application.ApiContracts.DepositSetting.Requests;

public class UpdateDepositSettingRequest
{
    public List<DepositSettingItemDto> Settings { get; set; } = new();
}

public class DepositSettingItemDto
{
    public string OrderType { get; set; } = string.Empty;

    public decimal OrderThreshold { get; set; }

    public int DepositRatio { get; set; }
}
