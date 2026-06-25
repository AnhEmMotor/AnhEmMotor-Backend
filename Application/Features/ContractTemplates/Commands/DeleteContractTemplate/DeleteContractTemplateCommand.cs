using Application.Common.Models;
using MediatR;

namespace Application.Features.ContractTemplates.Commands.DeleteContractTemplate;

public sealed record DeleteContractTemplateCommand(Guid Id) : IRequest<Result<Unit>>;
