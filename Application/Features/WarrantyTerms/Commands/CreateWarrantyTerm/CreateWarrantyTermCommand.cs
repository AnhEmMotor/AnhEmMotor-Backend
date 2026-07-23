using Application.ApiContracts.WarrantyTerms.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.WarrantyTerms.Commands.CreateWarrantyTerm;

public class CreateWarrantyTermCommand : CreateWarrantyTermRequest, IRequest<Result<int>>
{
}
