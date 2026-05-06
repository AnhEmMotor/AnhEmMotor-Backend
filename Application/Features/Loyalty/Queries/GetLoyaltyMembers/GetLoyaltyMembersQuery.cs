using Application.Common.Models;
using Application.Interfaces.Repositories.Lead;
using MediatR;

namespace Application.Features.Loyalty.Queries.GetLoyaltyMembers;

public sealed class GetLoyaltyMembersQuery : IRequest<Result<List<LoyaltyMemberResponse>>>
{
    public string? Search { get; set; }
}

public class LoyaltyMemberResponse
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Tier { get; set; } = string.Empty;

    public int Points { get; set; }
}

public sealed class GetLoyaltyMembersQueryHandler(ILeadReadRepository leadRepository) : IRequestHandler<GetLoyaltyMembersQuery, Result<List<LoyaltyMemberResponse>>>
{
    public async Task<Result<List<LoyaltyMemberResponse>>> Handle(
        GetLoyaltyMembersQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await leadRepository.GetLoyaltyMembersAsync(request.Search, cancellationToken);
        var members = entities
            .Select(
                l => new LoyaltyMemberResponse
                {
                    Id = l.Id,
                    FullName = l.FullName,
                    PhoneNumber = l.PhoneNumber,
                    Tier = l.Tier,
                    Points = l.Points
                })
            .ToList();
        return members;
    }
}
