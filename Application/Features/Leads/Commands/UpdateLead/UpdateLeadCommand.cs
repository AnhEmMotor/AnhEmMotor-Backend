using Application.Common.Models;
using Application.Interfaces.Repositories.Lead;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Leads.Commands.UpdateLead;

public record UpdateLeadCommand : IRequest<Result<int>>
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime? Birthday { get; set; }
    public string IdentificationNumber { get; set; } = string.Empty;
    public string AddressDetail { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string District { get; set; } = "Biên Hòa";
    public string Province { get; set; } = "Đồng Nai";
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string InterestedVehicle { get; set; } = string.Empty;
    public int Score { get; set; }
}

public class UpdateLeadCommandHandler(ILeadWriteRepository leadWriteRepository, ILeadReadRepository leadReadRepository) 
    : IRequestHandler<UpdateLeadCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await leadReadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (lead == null)
        {
            return Result<int>.Failure("Không tìm thấy khách hàng.");
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

        await leadWriteRepository.UpdateAsync(lead, cancellationToken);
        return lead.Id;
    }
}
