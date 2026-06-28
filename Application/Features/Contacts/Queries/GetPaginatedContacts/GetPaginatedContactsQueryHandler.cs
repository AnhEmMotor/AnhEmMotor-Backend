using Application.ApiContracts.Contacts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Contact;
using Domain.Primitives;
using MediatR;
using System.Linq;

namespace Application.Features.Contacts.Queries.GetPaginatedContacts;

public class GetPaginatedContactsQueryHandler(
    ISupportRequestRepository supportRequestRepository,
    ICustomerFeedbackRepository feedbackRepository,
    IJobApplicationRepository jobApplicationRepository) : IRequestHandler<GetPaginatedContactsQuery, Result<PagedResult<object>>>
{
    public async Task<Result<PagedResult<object>>> Handle(
        GetPaginatedContactsQuery request,
        CancellationToken cancellationToken)
    {
        var support = await supportRequestRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var feedback = await feedbackRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var applications = await jobApplicationRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
IEnumerable<object> items = request.ContactType?.ToLower() switch
{
"support" => ApplyAssignedUserFilter(ApplyStatusFilter(support, request.Status), request.AssignedUserId).Cast<object>(),
"feedback" => ApplyStatusFilter(feedback, request.Status).Cast<object>(),
"candidate" => ApplyStatusFilter(applications, request.Status).Cast<object>(),
_ => ApplyAssignedUserFilter(ApplyStatusFilter(support, request.Status), request.AssignedUserId)
.Cast<object>()
.Concat(ApplyStatusFilter(feedback, request.Status).Cast<object>())
.Concat(ApplyStatusFilter(applications, request.Status).Cast<object>())
};
        var total = items.Count();
        var paged = items.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

        var mappedPaged = paged.Select<object, object>(item =>
        {
            if (item is Domain.Entities.SupportRequest s)
            {
return new SupportRequestResponse
{
Id = s.Id,
ContactId = s.ContactId,
Subject = s.Subject,
Category = s.Category,
Email = s.Email,
OrderCode = s.OrderCode,
Content = s.Content,
Status = s.Status,
AssignedUserId = s.AssignedUserId,
AssignedUserName = s.AssignedUser?.FullName,
CreatedAt = s.CreatedAt,
                    Contact = s.Contact == null ? null : new ContactBasicResponse
                    {
                        Id = s.Contact.Id,
                        FullName = s.Contact.FullName,
                        Email = s.Contact.Email,
                        PhoneNumber = s.Contact.PhoneNumber,
                        InternalNote = s.Contact.InternalNote,
                        CreatedAt = s.Contact.CreatedAt,
                        Replies = s.Contact.Replies.Select(r => new ContactReplyResponse
                        {
                            Id = r.Id,
                            ContactId = r.ContactId,
                            Message = r.Message,
                            RepliedById = r.RepliedById,
                            RepliedByName = r.RepliedBy != null ? r.RepliedBy.FullName : null,
                            IsInternal = r.IsInternal,
                            CreatedAt = r.CreatedAt
                        }).ToList()
                    }
                };
            }
            if (item is Domain.Entities.CustomerFeedback f)
            {
                return new CustomerFeedbackResponse
                {
                    Id = f.Id,
                    ContactId = f.ContactId,
                    Rating = f.Rating,
                    FeedbackArea = f.FeedbackArea,
                    CustomerName = f.CustomerName,
                    PhoneNumber = f.PhoneNumber,
                    Content = f.Content,
                    Status = f.Status,
                    CreatedAt = f.CreatedAt,
                    Contact = f.Contact == null ? null : new ContactBasicResponse
                    {
                        Id = f.Contact.Id,
                        FullName = f.Contact.FullName,
                        Email = f.Contact.Email,
                        PhoneNumber = f.Contact.PhoneNumber,
                        InternalNote = f.Contact.InternalNote,
                        CreatedAt = f.Contact.CreatedAt,
                        Replies = f.Contact.Replies.Select(r => new ContactReplyResponse
                        {
                            Id = r.Id,
                            ContactId = r.ContactId,
                            Message = r.Message,
                            RepliedById = r.RepliedById,
                            RepliedByName = r.RepliedBy != null ? r.RepliedBy.FullName : null,
                            IsInternal = r.IsInternal,
                            CreatedAt = r.CreatedAt
                        }).ToList()
                    }
                };
            }
            if (item is Domain.Entities.JobApplication a)
            {
                return new JobApplicationResponse
                {
                    Id = a.Id,
                    ContactId = a.ContactId,
                    FullName = a.FullName,
                    Email = a.Email,
                    PhoneNumber = a.PhoneNumber,
                    AppliedPosition = a.AppliedPosition,
                    CvFileUrl = a.CvFileUrl,
                    CoverLetter = a.CoverLetter,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    Contact = a.Contact == null ? null : new ContactBasicResponse
                    {
                        Id = a.Contact.Id,
                        FullName = a.Contact.FullName,
                        Email = a.Contact.Email,
                        PhoneNumber = a.Contact.PhoneNumber,
                        InternalNote = a.Contact.InternalNote,
                        CreatedAt = a.Contact.CreatedAt,
                        Replies = a.Contact.Replies.Select(r => new ContactReplyResponse
                        {
                            Id = r.Id,
                            ContactId = r.ContactId,
                            Message = r.Message,
                            RepliedById = r.RepliedById,
                            RepliedByName = r.RepliedBy != null ? r.RepliedBy.FullName : null,
                            IsInternal = r.IsInternal,
                            CreatedAt = r.CreatedAt
                        }).ToList()
                    }
                };
            }
            return item;
        }).ToList();

        var result = new PagedResult<object>(mappedPaged, total, request.Page, request.PageSize);
        return Result<PagedResult<object>>.Success(result);
    }

    private static IEnumerable<T> ApplyAssignedUserFilter<T>(IEnumerable<T> items, Guid? assignedUserId) where T : class
{
    if (assignedUserId == null)
        return items;
    var prop = typeof(T).GetProperty("AssignedUserId");
    if (prop == null)
        return items;
    return items.Where(i => prop.GetValue(i)?.Equals(assignedUserId) == true);
}

private static IEnumerable<T> ApplyStatusFilter<T>(IEnumerable<T> items, string? status) where T : class
    {
        if (string.IsNullOrEmpty(status))
            return items;
        var statusProp = typeof(T).GetProperty("Status");
        if (statusProp == null)
            return items;
        return items.Where(
            i => statusProp.GetValue(i)?.ToString()?.Equals(status, StringComparison.OrdinalIgnoreCase) == true);
    }
}
