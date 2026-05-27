using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using Domain.Entities;
using MediatR;

namespace Application.Features.Contacts.Commands.CreateContact;

public class CreateContactCommandHandler(IContactInsertRepository contactInsertRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateContactCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = new Contact
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Subject = request.Subject,
            Message = request.Message,
            Status = "Pending"
        };
        contactInsertRepository.Add(contact);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(contact.Id);
    }
}
