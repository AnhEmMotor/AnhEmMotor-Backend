using Application.Interfaces.Repositories.Lead;
using Domain.Entities;
using MediatR;

namespace Application.Features.Leads.Commands.AddLeadActivity;

public record AddLeadActivityCommand(int LeadId, string ActivityType, string Description) : IRequest<int>;

public class AddLeadActivityCommandHandler(
    ILeadWriteRepository leadWriteRepository,
    ILeadReadRepository leadReadRepository) : IRequestHandler<AddLeadActivityCommand, int>
{
    public async Task<int> Handle(AddLeadActivityCommand request, CancellationToken cancellationToken)
    {
        var lead = await leadReadRepository.GetByIdAsync(request.LeadId, cancellationToken);
        if (lead == null)
            return 0;
        var activity = new LeadActivity
        {
            LeadId = request.LeadId,
            ActivityType = request.ActivityType,
            Description = request.Description
        };
        int scoreDelta = 0;
        string type = request.ActivityType.ToLower();
        string desc = request.Description.ToLower();
        if (type.Contains("call") || type.Contains("phone"))
        {
            if (desc.Contains("không nghe máy") || desc.Contains("missed"))
                scoreDelta = -10;
            else
                scoreDelta = 10;
        } else if (type.Contains("testdrive") || type.Contains("lái thử"))
        {
            scoreDelta = 20;
        } else if (desc.Contains("trả góp") || desc.Contains("installment"))
        {
            scoreDelta = 30;
        }
        lead.Score += scoreDelta;
        if (lead.Score < 0)
            lead.Score = 0;
        if (lead.Score > 100)
            lead.Score = 100;
        lead.Activities.Add(activity);
        await leadWriteRepository.UpdateAsync(lead, cancellationToken);
        return activity.Id;
    }
}
