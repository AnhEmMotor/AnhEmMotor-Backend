using Application.ApiContracts.ContractTemplate.Requests;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ContractTemplate;
using Domain.Entities;
using Domain.Enums;
using Domain.Primitives;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ContractTemplates.Commands.UpdateContractTemplate;

public class UpdateContractTemplateCommandHandler(
    IContractTemplateReadRepository contractTemplateReadRepository,
    IContractTemplateUpdateRepository contractTemplateUpdateRepository,
    IContractTemplateInsertRepository contractTemplateInsertRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateContractTemplateCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        UpdateContractTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await contractTemplateReadRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

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
            var (IsValid, ErrorMessage) = ValidateSyntax(request.Request.Content);
            if (!IsValid)
            {
                return Result<Unit>.Failure(Error.Validation(ErrorMessage!));
            }
        }

        entity.Name = request.Request.Name;
        entity.Type = request.Request.Type;
        entity.Code = request.Request.Code;
        entity.IsActive = request.Request.IsActive;
        entity.Status = (ContractTemplateStatus)request.Request.Status;
        entity.Content = request.Request.Content;
        entity.DynamicFields = request.Request.DynamicFields;

        contractTemplateUpdateRepository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var auditLog = new ContractTemplateAuditLog
        {
            Id = Guid.NewGuid(),
            ContractTemplateId = entity.Id,
            Action = "UpdateContent",
            ChangedBy = "System",
            Details = $"Cập nhật mẫu '{entity.Name}' v{entity.Version}"
        };
        contractTemplateInsertRepository.AddAuditLog(auditLog);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<Unit>.Success(Unit.Value);
    }

    private static (bool IsValid, string? ErrorMessage) ValidateSyntax(string htmlContent)
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
