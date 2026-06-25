using Application.Common.Models;
using MediatR;
using System.Text.RegularExpressions;

namespace Application.Features.ContractTemplates.Commands.ValidateContractTemplateSyntax;

public partial class ValidateContractTemplateSyntaxCommandHandler : IRequestHandler<ValidateContractTemplateSyntaxCommand, Result<bool>>
{
    private static readonly Regex PlaceholderRegex = MyRegex();

    public Task<Result<bool>> Handle(ValidateContractTemplateSyntaxCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var content = request.Content ?? string.Empty;
        foreach (Match match in PlaceholderRegex.Matches(content))
        {
            var inner = match.Value[2..^2];
            if (inner.Contains("{{") || inner.Contains("}}"))
            {
                return Task.FromResult(
                    Result<bool>.Failure(
                        Error.Failure(
                            "ContractTemplate.InvalidSyntax",
                            "Sai cú pháp từ khóa động. Vui lòng kiểm tra lại ký tự đóng mở ngoặc {{ }}.")));
            }
        }
        return Task.FromResult(Result<bool>.Success(true));
    }

    [GeneratedRegex(@"\{\{.+?\}\}", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex MyRegex();
}
