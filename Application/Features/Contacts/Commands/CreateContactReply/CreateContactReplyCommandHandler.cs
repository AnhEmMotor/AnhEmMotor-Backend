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
    IContactUpdateRepository contactUpdateRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUserContext) : IRequestHandler<CreateContactReplyCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateContactReplyCommand request, CancellationToken cancellationToken)
    {
        var contact = await contactReadRepository.GetByIdAsync(request.ContactId, cancellationToken)
            .ConfigureAwait(false);
        if (contact == null)
        {
            return Result<int>.Failure(Error.NotFound("Liên hệ không tồn tại."));
        }
        Guid userId = currentUserContext.GetUserId();
        var reply = new ContactReply { ContactId = contact.Id, Message = request.Message, RepliedById = userId };
        contactInsertRepository.AddReply(reply);
        if (request.MarkAsProcessed)
        {
            contact.Status = "Processed";
            contactUpdateRepository.Update(contact);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(reply.Id);
    }
}
