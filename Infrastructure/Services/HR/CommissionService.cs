using Application.Common.Models;
using Application.Interfaces.Services.HR;
using Domain.Entities.HR;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services.HR;

public class CommissionService(ApplicationDBContext context) : ICommissionService
{
    private async Task<CommissionPolicy?> GetActivePolicyAsync(
        int productId,
        int? categoryId,
        string? jobTitle,
        DateTimeOffset orderDate,
        CancellationToken ct)
    {
        return await context.CommissionPolicies
            .Where(p => p.IsActive && p.EffectiveDate <= orderDate)
            .Where(p => p.ProductId == productId || p.CategoryId == categoryId || p.TargetGroup == jobTitle)
            .OrderBy(p => p.ProductId == productId ? 0 : (p.CategoryId == categoryId ? 1 : 2))
            .ThenByDescending(p => p.EffectiveDate)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result> CalculatePendingCommissionAsync(
        int outputId,
        CancellationToken cancellationToken = default)
    {
        return await InternalCalculateAsync(outputId, CommissionStatus.Pending, cancellationToken);
    }

    public async Task<Result> CalculateAndRecordCommissionAsync(
        int outputId,
        CancellationToken cancellationToken = default)
    {
        var existingPending = await context.CommissionRecords
            .Where(r => r.OutputId == outputId && r.Status == CommissionStatus.Pending)
            .ToListAsync(cancellationToken);
        if (existingPending.Count > 0)
        {
            foreach (var record in existingPending)
            {
                record.Status = CommissionStatus.Confirmed;
                record.DateEarned = DateTime.UtcNow;
            }
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        return await InternalCalculateAsync(outputId, CommissionStatus.Confirmed, cancellationToken);
    }

    public async Task<Result> MarkCommissionAsPaidAsync(int outputId, CancellationToken cancellationToken = default)
    {
        var records = await context.CommissionRecords
            .Where(r => r.OutputId == outputId && r.Status == CommissionStatus.Confirmed)
            .ToListAsync(cancellationToken);
        if (records.Count == 0)
            return Result.Failure("Không tìm thấy hoa hồng đã xác nhận cho đơn hàng này.");
        foreach (var record in records)
        {
            record.Status = CommissionStatus.Paid;
            record.PaidAt = DateTime.UtcNow;
        }
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> VoidCommissionAsync(int outputId, CancellationToken cancellationToken = default)
    {
        var records = await context.CommissionRecords.Where(r => r.OutputId == outputId).ToListAsync(cancellationToken);
        if (records.Count > 0)
        {
            context.CommissionRecords.RemoveRange(records);
            await context.SaveChangesAsync(cancellationToken);
        }
        return Result.Success();
    }

    private async Task<Result> InternalCalculateAsync(int outputId, CommissionStatus targetStatus, CancellationToken ct)
    {
        var output = await context.OutputOrders
            .Include(o => o.OutputInfos)
            .ThenInclude(oi => oi.ProductVariant)
            .ThenInclude(pv => pv!.Product)
            .FirstOrDefaultAsync(o => o.Id == outputId, ct);
        if (output == null)
            return Result.Failure("Đơn hàng không tồn tại.");
        var salespersonId = output.CreatedBy;
        if (salespersonId == null)
            return Result.Failure("Không xác định được nhân viên kinh doanh.");
        var employeeProfile = await context.EmployeeProfiles.FirstOrDefaultAsync(p => p.UserId == salespersonId, ct);
        if (employeeProfile == null)
            return Result.Success();
        decimal totalCommission = 0;
        var notes = new StringBuilder();
        var snapshotItems = new List<object>();
        var orderDate = output.CreatedAt ?? DateTimeOffset.UtcNow;
        foreach (var item in output.OutputInfos)
        {
            if (item.ProductVariant?.Product == null || item.Count == null || item.Price == null)
                continue;
            var product = item.ProductVariant.Product;
            var policy = await GetActivePolicyAsync(
                product.Id,
                product.CategoryId,
                employeeProfile.JobTitle,
                orderDate,
                ct);
            if (policy == null)
                continue;
            decimal itemCommission = 0;
            string formula = string.Empty;
            if (policy.Type == "FixedAmount")
            {
                itemCommission = (decimal)item.Count * policy.Value;
                formula = $"{item.Count} {policy.Unit ?? "xe"} × {policy.Value:N0}đ = {itemCommission:N0}đ";
                notes.AppendLine($"- {product.Name}: {formula}");
            } else if (policy.Type == "Percentage")
            {
                var revenue = (decimal)item.Count * (decimal)item.Price;
                itemCommission = revenue * (policy.Value / 100);
                formula = $"{revenue:N0}đ × {policy.Value}% = {itemCommission:N0}đ";
                notes.AppendLine($"- {product.Name}: {formula}");
            }
            totalCommission += itemCommission;
            snapshotItems.Add(
                new
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    PolicyId = policy.Id,
                    PolicyName = policy.Name,
                    PolicyType = policy.Type,
                    PolicyValue = policy.Value,
                    PolicyEffectiveDate = policy.EffectiveDate,
                    Unit = policy.Unit,
                    Quantity = item.Count,
                    UnitPrice = item.Price,
                    Commission = itemCommission,
                    Formula = formula
                });
        }
        if (totalCommission > 0)
        {
            if (targetStatus == CommissionStatus.Pending)
            {
                var old = await context.CommissionRecords
                    .Where(r => r.OutputId == output.Id && r.Status == CommissionStatus.Pending)
                    .ToListAsync(ct);
                if (old.Count > 0)
                    context.CommissionRecords.RemoveRange(old);
            }
            var record = new CommissionRecord
            {
                EmployeeProfileId = employeeProfile.Id,
                OutputId = output.Id,
                Amount = totalCommission,
                Status = targetStatus,
                DateEarned = DateTime.UtcNow,
                PolicySnapshot = JsonSerializer.Serialize(snapshotItems),
                Note = notes.ToString().Trim()
            };
            await context.CommissionRecords.AddAsync(record, ct);
            await context.SaveChangesAsync(ct);
        }
        return Result.Success();
    }
}
