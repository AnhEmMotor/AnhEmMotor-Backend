using Application.Interfaces.Repositories.ServiceEvaluation;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.ServiceEvaluation;

public class ServiceEvaluationInsertRepository(ApplicationDBContext dbContext) : IServiceEvaluationInsertRepository
{
    public void Add(Domain.Entities.ServiceEvaluation evaluation)
    {
        dbContext.ServiceEvaluations.Add(evaluation);
    }
}

