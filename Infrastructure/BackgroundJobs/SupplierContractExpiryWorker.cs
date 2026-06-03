using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.BackgroundJobs;

public class SupplierContractExpiryWorker(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateExpiredContractsAsync(stoppingToken).ConfigureAwait(false);
                await SendExpiryWarningsAsync(stoppingToken).ConfigureAwait(false);
            }
            catch
            {
                // log error
            }
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task UpdateExpiredContractsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var now = DateTimeOffset.UtcNow;

        var expiredContracts = await context.SupplierContracts
            .IgnoreQueryFilters()
            .Where(c => c.Status == "Active" && c.ExpirationDate.HasValue && c.ExpirationDate.Value < now)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (expiredContracts.Count == 0) return;

        foreach (var contract in expiredContracts)
        {
            contract.Status = "Expired";
            context.SupplierContractAuditLogs.Add(new SupplierContractAuditLog
            {
                SupplierContractId = contract.Id,
                Action = "StatusChange",
                Details = $"Hợp đồng {contract.ContractNumber} tự động chuyển sang trạng thái Expired do hết hạn.",
                ChangedBy = "System (Background Worker)",
                OldValue = "Active",
                NewValue = "Expired"
            });
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task SendExpiryWarningsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var now = DateTimeOffset.UtcNow;
        var warningThreshold = now.AddDays(30);

        var expiringSoon = await context.SupplierContracts
            .IgnoreQueryFilters()
            .Where(c => c.Status == "Active"
                && c.ExpirationDate.HasValue
                && c.ExpirationDate.Value > now
                && c.ExpirationDate.Value <= warningThreshold)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var contract in expiringSoon)
        {
            var alreadyLogged = await context.SupplierContractAuditLogs
                .IgnoreQueryFilters()
                .AnyAsync(al => al.SupplierContractId == contract.Id
                    && al.Action == "ExpiryWarning"
                    && al.CreatedAt > now.AddDays(-1), cancellationToken)
                .ConfigureAwait(false);

            if (!alreadyLogged)
            {
                context.SupplierContractAuditLogs.Add(new SupplierContractAuditLog
                {
                    SupplierContractId = contract.Id,
                    Action = "ExpiryWarning",
                    Details = $"Hợp đồng {contract.ContractNumber} sắp hết hạn trong {(contract.ExpirationDate.Value - now.DateTime).Days} ngày.",
                    ChangedBy = "System (Background Worker)"
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
