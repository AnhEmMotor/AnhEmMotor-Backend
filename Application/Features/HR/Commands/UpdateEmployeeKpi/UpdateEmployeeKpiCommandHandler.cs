using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Employee;
using Application.Interfaces.Repositories.HR.Kpi;
using MediatR;

namespace Application.Features.HR.Commands.UpdateEmployeeKpi;

public sealed class UpdateEmployeeKpiCommandHandler(
    IEmployeeReadRepository employeeReadRepository,
    IEmployeeKpiRepository kpiRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateEmployeeKpiCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        UpdateEmployeeKpiCommand request,
        CancellationToken cancellationToken)
    {
        var kpi = await kpiRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (kpi is null)
        {
            return Result<int>.Failure("Không tìm thấy KPI cần cập nhật.");
        }
        var employee = await employeeReadRepository
            .GetByIdAsync(request.EmployeeProfileId, cancellationToken)
            .ConfigureAwait(false);
        if (employee is null)
        {
            return Result<int>.Failure("Không tìm thấy nhân viên được chọn.");
        }
        var metricName = request.MetricName.Trim();
        var isDuplicate = await kpiRepository
            .HasDuplicateAsync(
                request.EmployeeProfileId,
                metricName,
                request.PeriodStart,
                request.PeriodEnd,
                request.Id,
                cancellationToken)
            .ConfigureAwait(false);
        if (isDuplicate)
        {
            return Result<int>.Failure("KPI này đã tồn tại cho nhân viên trong cùng kỳ đánh giá.");
        }
        kpi.EmployeeProfileId = request.EmployeeProfileId;
        kpi.MetricName = metricName;
        kpi.TargetValue = request.TargetValue;
        kpi.ActualValue = request.ActualValue;
        kpi.PeriodStart = request.PeriodStart;
        kpi.PeriodEnd = request.PeriodEnd;
        kpi.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        kpiRepository.Update(kpi);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(kpi.Id);
    }
}
