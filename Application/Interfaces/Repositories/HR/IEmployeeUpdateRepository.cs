using Domain.Entities.HR;

namespace Application.Interfaces.Repositories.HR;

public interface IEmployeeUpdateRepository
{
    public void Update(EmployeeProfile employee);
}
