using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using MediatR;

namespace Application.Features.Contacts.Commands.UpdateInternalNote;

public class UpdateInternalNoteCommandHandler(
    IContactReadRepository contactReadRepository,
    IContactUpdateRepository contactUpdateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateInternalNoteCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateInternalNoteCommand request, CancellationToken cancellationToken)
    {
        var contact = await contactReadRepository.GetByIdAsync(request.ContactId, cancellationToken)
            .ConfigureAwait(false);
        if (contact == null)
        {
            return Result<bool>.Failure(Error.NotFound("Liên hệ không tồn tại."));
        }
        contact.InternalNote = request.InternalNote;
        contactUpdateRepository.Update(contact);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<bool>.Success(true);
    }
}
