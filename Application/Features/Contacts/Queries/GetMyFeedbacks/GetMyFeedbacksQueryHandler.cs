using Application.ApiContracts.Contacts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Contact;
using Application.Interfaces.Repositories.User;
using MediatR;

namespace Application.Features.Contacts.Queries.GetMyFeedbacks;

public class GetMyFeedbacksQueryHandler(
    ICustomerFeedbackRepository feedbackRepository,
    IUserReadRepository userReadRepository) : IRequestHandler<GetMyFeedbacksQuery, Result<List<CustomerFeedbackResponse>>>
{
    public async Task<Result<List<CustomerFeedbackResponse>>> Handle(
        GetMyFeedbacksQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userReadRepository.FindUserByIdAsync(request.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);
        if (user is null)
        {
            return Result<List<CustomerFeedbackResponse>>.Success([]);
        }

        var allFeedbacks = await feedbackRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var myFeedbacks = allFeedbacks
            .Where(f =>
                (!string.IsNullOrEmpty(user.PhoneNumber) && f.PhoneNumber == user.PhoneNumber) ||
                (!string.IsNullOrEmpty(user.Email) && f.Contact?.Email == user.Email))
            .OrderByDescending(f => f.CreatedAt)
            .ToList();

        var responses = myFeedbacks.Select(f => new CustomerFeedbackResponse
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
                    RepliedByName = r.RepliedBy?.FullName,
                    IsInternal = r.IsInternal,
                    CreatedAt = r.CreatedAt
                }).ToList()
            }
        }).ToList();

        return Result<List<CustomerFeedbackResponse>>.Success(responses);
    }
}
