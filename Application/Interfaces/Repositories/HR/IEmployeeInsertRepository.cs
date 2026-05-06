using Domain.Entities.HR;

namespace Application.Interfaces.Repositories.HR;

public interface IEmployeeInsertRepository
{
    public void Insert(EmployeeProfile employee);
}
