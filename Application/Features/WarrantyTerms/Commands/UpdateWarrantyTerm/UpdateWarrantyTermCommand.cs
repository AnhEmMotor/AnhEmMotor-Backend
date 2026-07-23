using Application.ApiContracts.WarrantyTerms.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.WarrantyTerms.Commands.UpdateWarrantyTerm;

public class UpdateWarrantyTermCommand : UpdateWarrantyTermRequest, IRequest<Result<int>>
{
    public int Id { get; set; }
}
