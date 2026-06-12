using Application.Common.Models;
using Application.ApiContracts.ContractTemplate.Requests;
using Domain.Primitives;
using MediatR;

namespace Application.Features.ContractTemplates.Commands.CreateContractTemplate;

public sealed record CreateContractTemplateCommand(CreateContractTemplateRequest Request) : IRequest<Result<Guid>>;
