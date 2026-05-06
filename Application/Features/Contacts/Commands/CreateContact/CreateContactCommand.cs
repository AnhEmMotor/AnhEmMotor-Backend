using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using Domain.Entities;
using MediatR;

namespace Application.Features.Contacts.Commands.CreateContact;

public record CreateContactCommand : IRequest<Result<int>>
{
    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Subject { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}

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
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(contact.Id);
    }
}
