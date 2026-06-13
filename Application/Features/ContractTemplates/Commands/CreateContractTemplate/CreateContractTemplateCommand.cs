using Application.ApiContracts.ContractTemplate.Requests;
using Application.Common.Models;
using MediatR;

namespace Application.Features.ContractTemplates.Commands.CreateContractTemplate;

public sealed record CreateContractTemplateCommand(CreateContractTemplateRequest Request) : IRequest<Result<Guid>>;
