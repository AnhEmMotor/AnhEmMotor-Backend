using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.Contacts.Queries.GetContacts;

public record GetContactsQuery : IRequest<Result<List<Contact>>>;

