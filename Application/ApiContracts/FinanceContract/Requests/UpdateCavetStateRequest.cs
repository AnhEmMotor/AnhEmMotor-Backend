namespace Application.ApiContracts.FinanceContract.Requests;

public class UpdateCavetStateRequest
{
    public string State { get; set; } = string.Empty;

    public DateTime? ReceivedDate { get; set; }

    public string? ReceiverName { get; set; }

    public string? StorageLocation { get; set; }
}

