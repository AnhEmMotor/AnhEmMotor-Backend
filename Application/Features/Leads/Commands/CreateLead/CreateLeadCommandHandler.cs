using Application.Common.Models;
using Application.Interfaces.Repositories.Lead;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Leads.Commands.CreateLead
{
    public class CreateLeadCommandHandler(ILeadWriteRepository leadWriteRepository, ILeadReadRepository leadReadRepository) : IRequestHandler<CreateLeadCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
        {
            // Check for duplicate identification number
            if (!string.IsNullOrEmpty(request.IdentificationNumber))
            {
                var existingLead = await leadReadRepository.GetByIdentificationNumberAsync(request.IdentificationNumber, cancellationToken).ConfigureAwait(false);
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

            await leadWriteRepository.AddAsync(lead, cancellationToken).ConfigureAwait(false);
            return lead.Id;
        }
    }
}
