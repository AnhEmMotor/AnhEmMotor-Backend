using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.HR.Employee;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListEmployeesForChat;

public class ListEmployeesForChatQueryHandler(
    IEmployeeReadRepository employeeReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<ListEmployeesForChatQuery, Result<ChatToolEnvelope<ChatEmployeeListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatEmployeeListItemDto>>> Handle(
        ListEmployeesForChatQuery request,
        CancellationToken cancellationToken)
    {
        var employees = await employeeReadRepository.GetAllWithUsersAsync(cancellationToken).ConfigureAwait(false);

        var keyword = request.Keyword?.Trim();
        var filtered = string.IsNullOrEmpty(keyword)
            ? employees
            : employees
                .Where(
                    e => e.User.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                        || e.JobTitle.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = filtered
            .Take(limit)
            .Select(
                e => new ChatEmployeeListItemDto
                {
                    EmployeeId = e.Id,
                    FullName = e.User.FullName,
                    JobTitle = e.JobTitle,
                    Status = e.User.Status
                })
            .ToList();

        var inner = new ChatToolResult<ChatEmployeeListItemDto>(dtos, filtered.Count, filtered.Count > dtos.Count);
        var filtersApplied = string.IsNullOrEmpty(keyword)
            ? new Dictionary<string, string>()
            : new Dictionary<string, string> { ["Từ khóa"] = keyword };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IEmployeeReadRepository.GetAllWithUsersAsync",
            filtersApplied,
            "nhan-vien",
            null);

        return ChatToolEnvelope<ChatEmployeeListItemDto>.Wrap(inner, meta);
    }
}
