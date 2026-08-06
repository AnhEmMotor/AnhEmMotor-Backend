using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Kpi;
using MediatR;

namespace Application.Features.HR.Commands.DeleteEmployeeKpi;

public sealed class DeleteEmployeeKpiCommandHandler(IEmployeeKpiRepository kpiRepository, IUnitOfWork unitOfWork) : IRequestHandler<DeleteEmployeeKpiCommand, Result<int>>
{
    public async Task<Result<int>> Handle(DeleteEmployeeKpiCommand request, CancellationToken cancellationToken)
    {
        var kpi = await kpiRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (kpi is null)
        {
            return Result<int>.Failure("Không tìm thấy KPI cần xóa.");
        }
        kpiRepository.Delete(kpi);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(kpi.Id);
    }
}
