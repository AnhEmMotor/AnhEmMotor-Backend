using Application.ApiContracts.Contacts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Contacts.Commands.CreateSupportRequest;

public class CreateSupportRequestCommandHandler(
    ISupportRequestRepository supportRequestRepository,
    IContactInsertRepository contactInsertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateSupportRequestCommand, Result<CreateSupportRequestResponse>>
{
    public async Task<Result<CreateSupportRequestResponse>> Handle(
        CreateSupportRequestCommand request,
        CancellationToken cancellationToken)
    {
        var contact = new Contact
        {
            FullName = request.Request.FullName ?? string.Empty,
            Email = request.Request.Email,
            PhoneNumber = request.Request.PhoneNumber ?? string.Empty,
            Subject = request.Request.Subject,
            Message = request.Request.Content,
            Status = "Pending"
        };
        contactInsertRepository.Add(contact);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var trackingToken = Guid.NewGuid();
        var supportRequest = new SupportRequest
        {
            ContactId = contact.Id,
            Subject = request.Request.Subject,
            Category = request.Request.Category,
            Email = request.Request.Email,
            OrderCode = request.Request.OrderCode,
            Content = request.Request.Content,
            Status = SupportRequestStatus.New,
            CustomerTrackingToken = trackingToken
        };
        await supportRequestRepository.AddAsync(supportRequest, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<CreateSupportRequestResponse>.Success(
            new CreateSupportRequestResponse
            {
                Id = supportRequest.Id,
                TrackingToken = trackingToken
            });
    }
}
