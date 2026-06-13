using Application.ApiContracts.Statistical.Responses;

namespace Application.Interfaces.Repositories.Statistical;

public interface IStatisticalAnalyticsRepository
{
    public Task<DashboardSummaryResponse> GetDashboardSummaryAsync(DateTime start, DateTime end, CancellationToken cancellationToken);

    public Task<PnlReportResponse> GetPnlReportAsync(int month, int year, CancellationToken cancellationToken);

    public Task<List<StaffPerformanceResponse>> GetStaffPerformanceAsync(DateTime start, DateTime end, CancellationToken cancellationToken);

    public Task<List<TransactionLogResponse>> GetRecentTransactionsAsync(int limit = 50, CancellationToken cancellationToken = default);
}
