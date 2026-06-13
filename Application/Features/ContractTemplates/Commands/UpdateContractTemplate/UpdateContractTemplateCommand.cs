using Application.ApiContracts.ContractTemplate.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.ContractTemplates.Commands.UpdateContractTemplate;

public sealed record UpdateContractTemplateCommand(Guid Id, UpdateContractTemplateRequest Request) : IRequest<Result<Unit>>;
