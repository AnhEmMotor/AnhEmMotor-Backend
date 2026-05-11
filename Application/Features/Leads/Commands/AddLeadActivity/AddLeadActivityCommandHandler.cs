using Domain.Entities;
using Domain.Constants.Lead;
using Mapster;
using MediatR;
using System;
using Application.Interfaces.Repositories.Lead.Lead;

namespace Application.Features.Leads.Commands.AddLeadActivity
{
    public class AddLeadActivityCommandHandler(
        ILeadUpdateRepository leadUpdateRepository,
        ILeadReadRepository leadReadRepository) : IRequestHandler<AddLeadActivityCommand, int>
    {
        public async Task<int> Handle(AddLeadActivityCommand request, CancellationToken cancellationToken)
        {
            var lead = await leadReadRepository.GetByIdAsync(request.LeadId, cancellationToken).ConfigureAwait(false);
            if (lead == null)
                return 0;
            var activity = request.Adapt<LeadActivity>();
            int scoreDelta = 0;
            string type = request.ActivityType.ToLower();
            string desc = request.Description.ToLower();
            if (type.Contains(LeadActivityKeywords.Call) || type.Contains(LeadActivityKeywords.Phone))
            {
                if (desc.Contains(LeadActivityKeywords.NoAnswer) || desc.Contains(LeadActivityKeywords.Missed))
                    scoreDelta = -10;
                else
                    scoreDelta = 10;
            } else if (type.Contains(LeadActivityKeywords.TestDriveEn) || type.Contains(LeadActivityKeywords.TestDriveVi))
            {
                scoreDelta = 20;
            } else if (desc.Contains(LeadActivityKeywords.InstallmentVi) || desc.Contains(LeadActivityKeywords.InstallmentEn))
            {
                scoreDelta = 30;
            }
            lead.Score += scoreDelta;
            if (lead.Score < 0)
                lead.Score = 0;
            if (lead.Score > 100)
                lead.Score = 100;
            lead.Activities.Add(activity);
            await leadUpdateRepository.UpdateAsync(lead, cancellationToken).ConfigureAwait(false);
            return activity.Id;
        }
    }
}
