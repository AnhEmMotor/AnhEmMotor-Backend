using Application.Common.Models;
using Application.ApiContracts.ContractTemplate.Responses;
using Domain.Primitives;
using MediatR;

namespace Application.Features.ContractTemplates.Queries.GetContractTemplateById;

public sealed record GetContractTemplateByIdQuery(Guid Id) : IRequest<Result<ContractTemplateResponse>>;
