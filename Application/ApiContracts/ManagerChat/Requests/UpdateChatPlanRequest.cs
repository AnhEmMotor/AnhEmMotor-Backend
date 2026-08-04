namespace Application.ApiContracts.ManagerChat.Requests;

public class UpdateChatPlanRequest
{
    public int Version { get; set; }

    public List<UpdatePlanStepOperation> Operations { get; set; } = [];
}
