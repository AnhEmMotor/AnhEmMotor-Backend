using Application.Interfaces.Repositories.HR.Employee;
using Domain.Entities;

using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.HR.Employee;

public class EmployeeInsertRepository(ApplicationDBContext context) : IEmployeeInsertRepository
{
    public void Insert(EmployeeProfile employee)
    {
        context.EmployeeProfiles.Add(employee);
    }
}
