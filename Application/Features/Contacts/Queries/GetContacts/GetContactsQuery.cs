using Application.Common.Models;
using Application.Interfaces.Repositories.Contact;
using Domain.Entities;
using MediatR;

namespace Application.Features.Contacts.Queries.GetContacts;

public record GetContactsQuery : IRequest<Result<List<Contact>>>;

public class GetContactsQueryHandler(IContactReadRepository contactReadRepository) : IRequestHandler<GetContactsQuery, Result<List<Contact>>>
{
    public async Task<Result<List<Contact>>> Handle(GetContactsQuery request, CancellationToken cancellationToken)
    {
        var contacts = await contactReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return Result<List<Contact>>.Success(contacts);
    }
}
