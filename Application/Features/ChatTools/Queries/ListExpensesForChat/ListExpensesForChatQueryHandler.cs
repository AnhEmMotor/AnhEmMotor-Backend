using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Features.Expenses.Responses;
using Application.Interfaces.Repositories.Expense;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.ListExpensesForChat;

public class ListExpensesForChatQueryHandler(
    IExpenseReadRepository expenseRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<ListExpensesForChatQuery, Result<ChatToolEnvelope<ChatExpenseListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatExpenseListItemDto>>> Handle(
        ListExpensesForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var sieveModel = new SieveModel { Page = 1, PageSize = limit, Sorts = "-ExpenseDate" };
        var paged = await expenseRepository
            .GetPagedAsync<ExpenseResponse>(sieveModel, cancellationToken)
            .ConfigureAwait(false);

        var dtos = (paged.Items ?? [])
            .Select(
                expense => new ChatExpenseListItemDto
                {
                    Name = expense.Name,
                    Amount = expense.Amount,
                    ExpenseDate = expense.ExpenseDate,
                    CategoryText = expense.CategoryText
                })
            .ToList();
        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatExpenseListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IExpenseReadRepository.GetPagedAsync",
            new Dictionary<string, string>(),
            "chi-phi",
            "VND");
        return ChatToolEnvelope<ChatExpenseListItemDto>.Wrap(inner, meta);
    }
}
