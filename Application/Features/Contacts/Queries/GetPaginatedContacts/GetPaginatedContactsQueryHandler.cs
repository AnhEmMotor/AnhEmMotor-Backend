using Application.ApiContracts.Contacts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Contact;
using Domain.Entities;
using Domain.Enums;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Contacts.Queries.GetPaginatedContacts;

public class GetPaginatedContactsQueryHandler(
    ISupportRequestRepository supportRequestRepository,
    ICustomerFeedbackRepository feedbackRepository,
    IJobApplicationRepository jobApplicationRepository)
: IRequestHandler<GetPaginatedContactsQuery, Result<PagedResult<object>>>
{
    public async Task<Result<PagedResult<object>>> Handle(GetPaginatedContactsQuery request, CancellationToken cancellationToken)
    {
        var support = await supportRequestRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var feedback = await feedbackRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var applications = await jobApplicationRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        IEnumerable<object> items = request.ContactType?.ToLower() switch
        {
            "support" => ApplyStatusFilter(support, request.Status).Cast<object>(),
            "feedback" => ApplyStatusFilter(feedback, request.Status).Cast<object>(),
            "candidate" => ApplyStatusFilter(applications, request.Status).Cast<object>(),
            _ => ApplyStatusFilter(support, request.Status).Cast<object>()
            .Concat(ApplyStatusFilter(feedback, request.Status).Cast<object>())
            .Concat(ApplyStatusFilter(applications, request.Status).Cast<object>())
        };

        var total = items.Count();
        var paged = items.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

        var result = new PagedResult<object>(paged, total, request.Page, request.PageSize);

        return Result<PagedResult<object>>.Success(result);
    }

    private static IEnumerable<T> ApplyStatusFilter<T>(IEnumerable<T> items, string? status) where T : class
    {
        if (string.IsNullOrEmpty(status)) return items;
        var statusProp = typeof(T).GetProperty("Status");
        if (statusProp == null) return items;
        return items.Where(i => statusProp.GetValue(i)?.ToString()?.Equals(status, StringComparison.OrdinalIgnoreCase) == true);
    }
}
