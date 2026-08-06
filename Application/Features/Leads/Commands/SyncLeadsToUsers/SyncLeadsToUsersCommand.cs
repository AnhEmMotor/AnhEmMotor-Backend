using Application.Common.Models;
using MediatR;

namespace Application.Features.Leads.Commands.SyncLeadsToUsers;

public record SyncLeadsToUsersCommand : IRequest<Result<int>>;
