using MediatR;
using Application.Common.Models;
namespace Application.Features.ContractTemplates.Commands.ValidateContractTemplateSyntax;

public sealed record ValidateContractTemplateSyntaxCommand(string Content) : IRequest<Result<bool>>;
