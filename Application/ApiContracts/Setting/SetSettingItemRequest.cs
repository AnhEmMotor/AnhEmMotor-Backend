using Application.ValidationAttributes;

namespace Application.ApiContracts.Setting
{
    public class SetSettingItemRequest
    {
        [InList("deposit_ratio,inventory_alert_level,order_value_exceeds,z-bike_threshold_for_meeting", ErrorMessage = "Key khong hop le.")]
        public string? Key { get; set; }

        public long? Value { get; set; }
    }
}
