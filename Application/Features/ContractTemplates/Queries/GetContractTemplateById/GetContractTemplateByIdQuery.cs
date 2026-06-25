using Application.ApiContracts.ContractTemplate.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.ContractTemplates.Queries.GetContractTemplateById;

public sealed record GetContractTemplateByIdQuery(Guid Id) : IRequest<Result<ContractTemplateResponse>>;
