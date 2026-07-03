using Application.Common.Models;
using Application.Interfaces.Repositories.Lead.Lead;
using MediatR;
using System;

namespace Application.Features.Leads.Commands.UpdateLead
{
    public class UpdateLeadCommandHandler(
        ILeadUpdateRepository leadUpdateRepository,
        ILeadReadRepository leadReadRepository) : IRequestHandler<UpdateLeadCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(UpdateLeadCommand request, CancellationToken cancellationToken)
        {
            var lead = await leadReadRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (lead == null)
            {
                return Result<int>.Failure("Không tìm thấy khách hàng.");
            }
            if (!string.IsNullOrEmpty(request.IdentificationNumber) &&
                string.Compare(request.IdentificationNumber, lead.IdentificationNumber) != 0)
            {
                var existingWithCccd = await leadReadRepository.GetByIdentificationNumberAsync(
                    request.IdentificationNumber,
                    cancellationToken)
                    .ConfigureAwait(false);
                if (existingWithCccd != null && existingWithCccd.Id != lead.Id)
                {
                    return Result<int>.Failure("Identification number already exists.");
                }
            }
            lead.FullName = request.FullName;
            lead.Email = request.Email;
            lead.PhoneNumber = request.PhoneNumber;
            lead.Gender = request.Gender;
            lead.Birthday = request.Birthday;
            lead.IdentificationNumber = request.IdentificationNumber;
            lead.AddressDetail = request.AddressDetail;
            lead.Ward = request.Ward;
            lead.District = request.District;
            lead.Province = request.Province;
            if (!string.IsNullOrEmpty(request.Status))
            {
                lead.Status = request.Status;
            }
            if (!string.IsNullOrEmpty(request.Source))
            {
                lead.Source = request.Source;
            }
lead.InterestedVehicle = request.InterestedVehicle;
lead.Score = request.Score;
lead.IsVerified = request.IsVerified;
await leadUpdateRepository.UpdateAsync(lead, cancellationToken).ConfigureAwait(false);
            return lead.Id;
        }
    }
}
