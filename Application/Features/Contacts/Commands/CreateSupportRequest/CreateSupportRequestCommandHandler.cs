using Application.ApiContracts.Contacts.Requests;
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
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSupportRequestCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateSupportRequestCommand request, CancellationToken cancellationToken)
    {
        var contact = new Contact
        {
            FullName = string.Empty,
            Email = request.Request.Email,
            PhoneNumber = string.Empty,
            Subject = request.Request.Subject,
            Message = request.Request.Content,
            Status = "Pending"
        };

        contactInsertRepository.Add(contact);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var supportRequest = new SupportRequest
        {
            ContactId = contact.Id,
            Subject = request.Request.Subject,
            Category = request.Request.Category,
            Email = request.Request.Email,
            OrderCode = request.Request.OrderCode,
            Content = request.Request.Content,
            Status = SupportRequestStatus.New
        };

        await supportRequestRepository.AddAsync(supportRequest, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<int>.Success(supportRequest.Id);
    }
}
