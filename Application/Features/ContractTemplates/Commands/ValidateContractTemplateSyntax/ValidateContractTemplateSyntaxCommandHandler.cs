using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using System.Text.RegularExpressions;

namespace Application.Features.ContractTemplates.Commands.ValidateContractTemplateSyntax;

internal sealed class ValidateContractTemplateSyntaxCommandHandler
    : IRequestHandler<ValidateContractTemplateSyntaxCommand, Result<bool>>
{
    private static readonly Regex PlaceholderRegex = new(
        @"\{\{.+?\}\}",
        RegexOptions.Compiled | RegexOptions.Singleline);

    public Task<Result<bool>> Handle(
        ValidateContractTemplateSyntaxCommand request,
        CancellationToken cancellationToken)
    {
        var content = request.Content ?? string.Empty;

        foreach (Match match in PlaceholderRegex.Matches(content))
        {
            var inner = match.Value.Substring(2, match.Value.Length - 4);

            if (inner.Contains("{{") || inner.Contains("}}"))
            {
                return Task.FromResult(Result<bool>.Failure(Error.Failure(
                    "ContractTemplate.InvalidSyntax",
                    "Sai cú pháp từ khóa động. Vui lòng kiểm tra lại ký tự đóng mở ngoặc {{ }}.")));
            }
        }

        return Task.FromResult(Result<bool>.Success(true));
    }
}
