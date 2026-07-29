using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Employee;
using Application.Interfaces.Repositories.HR.Kpi;
using Domain.Entities;
using MediatR;

namespace Application.Features.HR.Commands.CreateEmployeeKpi;

public sealed class CreateEmployeeKpiCommandHandler(
    IEmployeeReadRepository employeeReadRepository,
    IEmployeeKpiRepository kpiRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateEmployeeKpiCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateEmployeeKpiCommand request,
        CancellationToken cancellationToken)
    {
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
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (isDuplicate)
        {
            return Result<int>.Failure("KPI này đã tồn tại cho nhân viên trong cùng kỳ đánh giá.");
        }
        var kpi = new KPI
        {
            EmployeeProfileId = request.EmployeeProfileId,
            MetricName = metricName,
            TargetValue = request.TargetValue,
            ActualValue = request.ActualValue,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim()
        };
        await kpiRepository.AddAsync(kpi, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(kpi.Id);
    }
}
