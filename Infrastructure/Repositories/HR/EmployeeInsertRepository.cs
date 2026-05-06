using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.HR;

public class EmployeeInsertRepository(ApplicationDBContext context) : IEmployeeInsertRepository
{
    public void Insert(EmployeeProfile employee)
    {
        context.EmployeeProfiles.Add(employee);
    }
}
