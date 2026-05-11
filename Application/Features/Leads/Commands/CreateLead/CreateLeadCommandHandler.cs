using Application.Common.Models;
using Application.Interfaces.Repositories.Lead.Lead;
using Domain.Entities;
using MediatR;
using System;

namespace Application.Features.Leads.Commands.CreateLead
{
    public class CreateLeadCommandHandler(
        ILeadInsertRepository leadInsertRepository,
        ILeadReadRepository leadReadRepository) : IRequestHandler<CreateLeadCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.IdentificationNumber))
            {
                var existingLead = await leadReadRepository.GetByIdentificationNumberAsync(
                    request.IdentificationNumber,
                    cancellationToken)
                    .ConfigureAwait(false);
                if (existingLead != null)
                {
                    return Result<int>.Failure("Identification number already exists.");
                }
            }
            var lead = new Lead
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                IdentificationNumber = request.IdentificationNumber,
                Birthday = request.Birthday,
                Gender = request.Gender
            };
            await leadInsertRepository.AddAsync(lead, cancellationToken).ConfigureAwait(false);
            return lead.Id;
        }
    }
}
