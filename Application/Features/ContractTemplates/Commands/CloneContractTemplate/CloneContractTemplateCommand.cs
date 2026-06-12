using Application.Common.Models;
using Domain.Primitives;
using MediatR;

namespace Application.Features.ContractTemplates.Commands.CloneContractTemplate;

public sealed record CloneContractTemplateCommand(Guid Id) : IRequest<Result<Guid>>;
