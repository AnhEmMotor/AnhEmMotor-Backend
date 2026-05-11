using Application.Common.Models;
using Domain.Primitives;
using Application.Interfaces.Repositories;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;
using Application.Interfaces.Repositories.Lead.Lead;

namespace Infrastructure.Repositories.Lead.Lead;

public class LeadReadRepository(ApplicationDBContext context, ISievePaginator paginator) : ILeadReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<Domain.Entities.Lead, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable();
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return paginator.ApplyAsync<Domain.Entities.Lead, TResponse>(query, sieveModel, mode, cancellationToken);
    }
    public Task<Domain.Entities.Lead?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return context.Leads.Include(l => l.Activities).FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public Task<Domain.Entities.Lead?> GetByPhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        return context.Leads
            .Include(l => l.Activities)
            .FirstOrDefaultAsync(l => string.Compare(l.PhoneNumber, phoneNumber) == 0, cancellationToken);
    }

    public Task<List<Domain.Entities.Lead>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return context.Leads.Include(l => l.Activities).OrderByDescending(l => l.Score).ToListAsync(cancellationToken);
    }

    public Task<List<Domain.Entities.Lead>> GetAllLeadsWithActivitiesAsync(
        CancellationToken cancellationToken = default)
    {
        return context.Leads
            .Include(l => l.Activities)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Domain.Entities.Lead>> GetLoyaltyMembersAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = context.Leads.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(l => l.FullName.Contains(search) || l.PhoneNumber.Contains(search));
        }
        return query
            .OrderByDescending(l => l.Points)
            .ToListAsync(cancellationToken);
    }

    public Task<Domain.Entities.Lead?> GetByIdentificationNumberAsync(
        string identificationNumber,
        CancellationToken cancellationToken = default)
    {
        return context.Leads
            .FirstOrDefaultAsync(
                l => string.Compare(l.IdentificationNumber, identificationNumber) == 0,
                cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return context.Leads.AnyAsync(l => l.Id == id, cancellationToken);
    }

    internal IQueryable<Domain.Entities.Lead> GetQueryable()
    {
        return context.Leads.AsQueryable();
    }
}
