using Application.Common.Models;
using MediatR;

namespace Application.Features.ContractTemplates.Commands.ValidateContractTemplateSyntax;

public sealed record ValidateContractTemplateSyntaxCommand(string Content) : IRequest<Result<bool>>;
