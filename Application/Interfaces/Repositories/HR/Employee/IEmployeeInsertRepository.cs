using Domain.Entities;

namespace Application.Interfaces.Repositories.HR.Employee;

public interface IEmployeeInsertRepository
{
    public void Insert(EmployeeProfile employee);
}
