using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Voucher;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.Voucher;

public class VoucherReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IVoucherReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<Domain.Entities.Voucher, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.GetQuery<Domain.Entities.Voucher>(mode)
            .Include(v => v.VoucherLeads)
            .AsQueryable();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return paginator.ApplyAsync<Domain.Entities.Voucher, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    public Task<Domain.Entities.Voucher?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.Vouchers
            .Include(v => v.VoucherLeads)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return context.Vouchers.AnyAsync(v => v.Code == code, cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(string code, int excludeId, CancellationToken cancellationToken)
    {
        return context.Vouchers.AnyAsync(v => v.Code == code && v.Id != excludeId, cancellationToken);
    }
}
