namespace Application.ApiContracts.Shipping.Requests;

public class GhtkWebhookRequest
{
    public string partner_id { get; set; } = string.Empty;
    public string label_id { get; set; } = string.Empty;
    public int status_id { get; set; }
    public string action_time { get; set; } = string.Empty;
    public string reason_code { get; set; } = string.Empty;
    public string reason { get; set; } = string.Empty;
    public int weight { get; set; }
    public int fee { get; set; }
    public int return_part_package { get; set; }
}
