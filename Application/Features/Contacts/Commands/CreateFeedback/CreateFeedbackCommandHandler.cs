using Application.ApiContracts.Contacts.Requests;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Contacts.Commands.CreateFeedback;

public class CreateFeedbackCommandHandler(
    ICustomerFeedbackRepository feedbackRepository,
    IContactInsertRepository contactInsertRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateFeedbackCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateFeedbackCommand request, CancellationToken cancellationToken)
    {
        var contact = new Contact
        {
            FullName = request.Request.CustomerName,
            Email = string.Empty,
            PhoneNumber = request.Request.PhoneNumber,
            Subject = $"Đánh giá {request.Request.FeedbackArea} - {request.Request.Rating} sao",
            Message = request.Request.Content,
            Status = "Pending"
        };

        contactInsertRepository.Add(contact);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var feedback = new CustomerFeedback
        {
            ContactId = contact.Id,
            Rating = request.Request.Rating,
            FeedbackArea = request.Request.FeedbackArea,
            CustomerName = request.Request.CustomerName,
            PhoneNumber = request.Request.PhoneNumber,
            Content = request.Request.Content,
            Status = FeedbackStatus.Pending
        };

        await feedbackRepository.AddAsync(feedback, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<int>.Success(feedback.Id);
    }
}
