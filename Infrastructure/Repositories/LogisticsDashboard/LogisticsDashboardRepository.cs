using Application.ApiContracts.Logistics.Responses;
using Application.Features.Logistics.Queries.GetLogisticsDashboard;
using Application.Interfaces.Repositories.LogisticsDashboard;
using Infrastructure.DBContexts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.LogisticsDashboard;

public class LogisticsDashboardRepository : ILogisticsDashboardRepository
{
    private readonly ApplicationDBContext _context;

    public LogisticsDashboardRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<LogisticsDashboardResponse> GetDashboardAsync(DateTime fromDate, CancellationToken cancellationToken)
    {
        var response = new LogisticsDashboardResponse();

        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;

        if (wasClosed)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "[dbo].[sp_LogisticsDashboard]";
            command.CommandType = CommandType.StoredProcedure;

            var param = command.CreateParameter();
            param.ParameterName = "@FromDate";
            param.Value = fromDate;
            command.Parameters.Add(param);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            // 1. Summary Cards
            if (await reader.ReadAsync(cancellationToken))
            {
                response.Summary = new LogisticsDashboardSummaryResponse
                {
                    FulfillmentWorkload = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    FulfillmentWorkloadIsOverload = reader.IsDBNull(1) ? false : reader.GetInt32(1) == 1,
                    PendingUnreconciledCod = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                    OtifRate = reader.IsDBNull(3) ? 0.0 : reader.GetDouble(3),
                    ReturnsClaimsRate = reader.IsDBNull(4) ? 0.0 : reader.GetDouble(4)
                };
            }

            // 2. Fulfillment Funnel
            if (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    var statusName = reader.GetString(0);
                    var count = reader.GetInt32(1);
                    response.FulfillmentFunnel[statusName] = count;
                }
            }

            // 3. Trends (14 days)
            if (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    response.Trends.Add(new LogisticsTrendPointResponse
                    {
                        DayLabel = reader.GetString(0),
                        DeliveredCount = reader.GetInt32(1),
                        ShippingCost = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2)
                    });
                }
            }

            // 4. Carrier Scorecard
            if (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    response.CarrierScorecard.Add(new CarrierScoreRowResponse
                    {
                        Carrier = reader.GetString(0),
                        DeliveredCount = reader.GetInt32(1),
                        AvgDeliveryDays = reader.GetDouble(2),
                        AvgShippingCostPerOrder = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                        ReturnsRatio = reader.GetDouble(4)
                    });
                }
            }

            // 5a. ngam_kho
            if (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    response.Exceptions.Add(new LogisticsExceptionRowResponse
                    {
                        Type = reader.GetString(0),
                        TrackingNumber = reader.GetString(1),
                        Message = reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3)
                    });
                }
            }

            // 5b. giao_cham
            if (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    response.Exceptions.Add(new LogisticsExceptionRowResponse
                    {
                        Type = reader.GetString(0),
                        TrackingNumber = reader.GetString(1),
                        Message = reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3)
                    });
                }
            }

            // 5c. hoan_cho_kiem_tra
            if (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    response.Exceptions.Add(new LogisticsExceptionRowResponse
                    {
                        Type = reader.GetString(0),
                        TrackingNumber = reader.GetString(1),
                        Message = reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3)
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

        return response;
    }
}
