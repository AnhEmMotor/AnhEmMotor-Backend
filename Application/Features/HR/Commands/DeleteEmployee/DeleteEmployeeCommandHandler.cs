using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Employee;
using MediatR;

namespace Application.Features.HR.Commands.DeleteEmployee;

public sealed class DeleteEmployeeCommandHandler(
    IEmployeeReadRepository employeeReadRepository,
    IEmployeeDeleteRepository employeeDeleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteEmployeeCommand, Result<int>>
{
    public async Task<Result<int>> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeReadRepository.GetByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (employee is null)
        {
            return Result<int>.Failure("Không tìm thấy hồ sơ nhân viên.");
        }

        employeeDeleteRepository.Delete(employee);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(employee.Id);
    }
}
