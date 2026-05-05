using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Contacts.Commands.CreateContactReply;

public class CreateContactReplyCommandHandler(
    IContactReadRepository contactReadRepository,
    IContactInsertRepository contactInsertRepository,
    IUnitOfWork unitOfWork,
    IHttpTokenAccessorService tokenAccessor) : IRequestHandler<CreateContactReplyCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateContactReplyCommand request, CancellationToken cancellationToken)
    {
        var contact = await contactReadRepository.GetByIdAsync(request.ContactId, cancellationToken).ConfigureAwait(false);
        if (contact == null)
        {
            return Result<int>.Failure(Error.NotFound("Liên hệ không tồn tại."));
        }
        var userIdString = tokenAccessor.GetUserId();
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Result<int>.Failure(Error.Unauthorized("Không thể xác định người dùng thực hiện phản hồi."));
        }
        var reply = new ContactReply { ContactId = contact.Id, Message = request.Message, RepliedById = userId };
        contactInsertRepository.AddReply(reply);
        if (request.MarkAsProcessed)
        {
            contact.Status = "Processed";
            contactInsertRepository.Update(contact);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(reply.Id);
    }
}
