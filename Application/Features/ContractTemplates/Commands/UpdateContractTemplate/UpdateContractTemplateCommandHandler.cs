using Application.ApiContracts.ContractTemplate.Requests;
using Application.Common.Models;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ContractTemplates.Commands.UpdateContractTemplate;

internal sealed class UpdateContractTemplateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateContractTemplateCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        UpdateContractTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ContractTemplates
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            return Result<Unit>.Failure(Error.NotFound("Mẫu hợp đồng không tồn tại."));
        }

        if (entity.IsUsed)
        {
            return Result<Unit>.Failure(Error.Forbidden(
                "Mẫu hợp đồng đã phát sinh hợp đồng thực tế không thể sửa trực tiếp. Vui lòng sử dụng chức năng 'Nhân bản tạo phiên bản mới'."));
        }

        if (request.Request.Content is { Length: > 0 })
        {
            var validationResult = ValidateSyntax(request.Request.Content);
            if (!validationResult.IsValid)
            {
                return Result<Unit>.Failure(Error.Validation(validationResult.ErrorMessage!));
            }
        }

        entity.Name = request.Request.Name;
        entity.Type = request.Request.Type;
        entity.Code = request.Request.Code;
        entity.IsActive = request.Request.IsActive;
        entity.Status = (ContractTemplateStatus)request.Request.Status;
        entity.Content = request.Request.Content;
        entity.DynamicFields = request.Request.DynamicFields;

        await context.SaveChangesAsync(cancellationToken);

        var auditLog = new ContractTemplateAuditLog
        {
            Id = Guid.NewGuid(),
            ContractTemplateId = entity.Id,
            Action = "UpdateContent",
            ChangedBy = "System",
            Details = $"Cập nhật mẫu '{entity.Name}' v{entity.Version}"
        };
        await context.ContractTemplateAuditLogs.AddAsync(auditLog, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }

    private (bool IsValid, string? ErrorMessage) ValidateSyntax(string htmlContent)
    {
        var pattern = @"\{\{(.+?)\}\}";
        var matches = System.Text.RegularExpressions.Regex.Matches(htmlContent, pattern);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var inner = match.Groups[1].Value;
            if (inner.Contains("{{") || inner.Contains("}}"))
            {
                return (false, "Sai cú pháp từ khóa động. Vui lòng kiểm tra lại ký tự đóng mở ngoặc {{ }}.");
            }
        }

        return (true, null);
    }
}
