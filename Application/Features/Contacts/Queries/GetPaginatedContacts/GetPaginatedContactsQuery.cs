using Application.Common.Models;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Contacts.Queries.GetPaginatedContacts;

public record GetPaginatedContactsQuery(string? ContactType, string? Status, int Page = 1, int PageSize = 20) : IRequest<Result<PagedResult<object>>>
{
}
