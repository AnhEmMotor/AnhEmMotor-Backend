using Application.Features.Statistical.DTOs;
using Application.Interfaces.Repositories.WorkshopDashboard;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.WorkshopDashboard;

public class WorkshopDashboardRepository : IWorkshopDashboardRepository
{
    private readonly ApplicationDBContext _context;

    public WorkshopDashboardRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<WorkshopDashboardDto> GetOverviewAsync(DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken)
    {
        var result = new WorkshopDashboardDto();

        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;

        if (wasClosed)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "[dbo].[sp_WorkshopDashboard]";
            command.CommandType = CommandType.StoredProcedure;

            var startDateParam = command.CreateParameter();
            startDateParam.ParameterName = "@StartDate";
            startDateParam.Value = startDate;
            command.Parameters.Add(startDateParam);

            var endDateParam = command.CreateParameter();
            endDateParam.ParameterName = "@EndDate";
            endDateParam.Value = endDate;
            command.Parameters.Add(endDateParam);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            // 1. Summary Cards
            if (await reader.ReadAsync(cancellationToken))
            {
                result.SummaryCards = new SummaryCardsDto
                {
                    TotalBookings = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    TotalRepairOrders = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    TotalMaintenances = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    TotalWarrantyClaims = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    AvgCompletionHours = reader.IsDBNull(4) ? 0.0 : reader.GetDouble(4)
                };
            }

            // 2. Financial Summary
            if (await reader.NextResultAsync(cancellationToken) && await reader.ReadAsync(cancellationToken))
            {
                result.FinancialSummary = new FinancialSummaryDto
                {
                    TotalRevenue = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0),
                    TotalUnpaidAmount = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                    TotalPartialAmount = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                    UnpaidInvoicesCount = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                };
            }

            // 3. Daily Revenues
            if (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.DailyRevenues.Add(new DailyRevenueDto
                    {
                        RevenueDate = reader.GetDateTime(0),
                        DailyRevenue = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1)
                    });
                }
            }

            // 4. Top Services
            if (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.TopServices.Add(new TopServiceDto
                    {
                        ServiceName = reader.GetString(0),
                        UsageCount = reader.GetInt32(1),
                        TotalServiceRevenue = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2)
                    });
                }
            }

            // 5. Status Breakdown
            if (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    result.StatusBreakdowns.Add(new StatusBreakdownDto
                    {
                        Status = reader.GetString(0),
                        StatusCount = reader.GetInt32(1)
                    });
                }
            }
        }
        finally
        {
            if (wasClosed)
            {
                await connection.CloseAsync();
            }
        }

        return result;
    }
}
