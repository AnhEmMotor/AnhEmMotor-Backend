using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.HR;

public class EmployeeUpdateRepository(ApplicationDBContext context) : IEmployeeUpdateRepository
{
    public void Update(EmployeeProfile employee)
    {
        context.EmployeeProfiles.Update(employee);
    }
}
