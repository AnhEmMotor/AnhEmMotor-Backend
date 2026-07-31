namespace Application.ApiContracts.ManagerChat.Requests;

/// <summary>Body dùng chung cho approve/reject plan — chỉ cần version để check optimistic concurrency.</summary>
public class ChatPlanVersionRequest
{
    public int Version { get; set; }
}
