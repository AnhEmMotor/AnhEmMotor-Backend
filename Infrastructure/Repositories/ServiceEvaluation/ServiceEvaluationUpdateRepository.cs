using Application.Interfaces.Repositories.ServiceEvaluation;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.ServiceEvaluation;

public class ServiceEvaluationUpdateRepository(ApplicationDBContext dbContext) : IServiceEvaluationUpdateRepository
{
    public void Update(Domain.Entities.ServiceEvaluation evaluation)
    {
        dbContext.ServiceEvaluations.Update(evaluation);
    }
}

