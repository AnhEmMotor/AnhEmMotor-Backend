using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Employee;
using MediatR;

namespace Application.Features.HR.Commands.DeleteEmployee;

public sealed class DeleteEmployeeCommandHandler(
    IEmployeeReadRepository employeeReadRepository,
    IEmployeeDeleteRepository employeeDeleteRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteEmployeeCommand, Result>
{
    public async Task<Result> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeReadRepository
            .GetByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (employee is null)
            return Result.Failure("Không tìm thấy hồ sơ nhân sự.");
        employeeDeleteRepository.Delete(employee);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
