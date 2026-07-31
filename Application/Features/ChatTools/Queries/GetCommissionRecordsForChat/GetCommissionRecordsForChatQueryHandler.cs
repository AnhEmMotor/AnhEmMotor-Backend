using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.HR.Commission;
using Application.Interfaces.Repositories.HR.Employee;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetCommissionRecordsForChat;

public class GetCommissionRecordsForChatQueryHandler(
    ICommissionReadRepository commissionReadRepository,
    IEmployeeReadRepository employeeReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetCommissionRecordsForChatQuery, Result<ChatToolEnvelope<ChatCommissionRecordItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatCommissionRecordItemDto>>> Handle(
        GetCommissionRecordsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var records = request.EmployeeId.HasValue
            ? await commissionReadRepository
                .GetRecordsByEmployeeIdAsync(request.EmployeeId.Value, cancellationToken)
                .ConfigureAwait(false)
            : await commissionReadRepository.GetRecordsAsync(cancellationToken).ConfigureAwait(false);

        var employees = await employeeReadRepository.GetAllWithUsersAsync(cancellationToken).ConfigureAwait(false);
        var namesById = employees.ToDictionary(e => e.Id, e => e.User?.FullName ?? "Unknown");

        var ordered = records.OrderByDescending(r => r.DateEarned).ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = ordered
            .Take(limit)
            .Select(
                r => new ChatCommissionRecordItemDto
                {
                    EmployeeId = r.EmployeeProfileId,
                    EmployeeName = namesById.GetValueOrDefault(r.EmployeeProfileId, "Unknown"),
                    OutputId = r.OutputId,
                    Amount = r.Amount,
                    Status = r.Status.ToString(),
                    DateEarned = r.DateEarned,
                    PaidAt = r.PaidAt
                })
            .ToList();

        var inner = new ChatToolResult<ChatCommissionRecordItemDto>(dtos, ordered.Count, ordered.Count > dtos.Count);
        var filtersApplied = request.EmployeeId.HasValue
            ? new Dictionary<string, string> { ["Mã nhân viên"] = request.EmployeeId.Value.ToString() }
            : new Dictionary<string, string>();

        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            request.EmployeeId.HasValue
                ? "ICommissionReadRepository.GetRecordsByEmployeeIdAsync"
                : "ICommissionReadRepository.GetRecordsAsync",
            filtersApplied,
            "hoa-hong-nhan-vien",
            "VND");

        return ChatToolEnvelope<ChatCommissionRecordItemDto>.Wrap(inner, meta);
    }
}
