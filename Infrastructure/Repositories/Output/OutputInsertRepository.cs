using Application.Interfaces.Repositories.Output;
using Infrastructure.DBContexts;
using OutputEntity = Domain.Entities.Output;

namespace Infrastructure.Repositories.Output;

public class OutputInsertRepository(ApplicationDBContext context) : IOutputInsertRepository
{
    public void Add(OutputEntity output)
    {
        context.OutputOrders.Add(output);
    }
}
