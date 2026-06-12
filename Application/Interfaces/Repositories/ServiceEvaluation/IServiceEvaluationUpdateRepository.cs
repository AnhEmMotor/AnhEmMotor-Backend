namespace Application.Interfaces.Repositories.ServiceEvaluation;

using Domain.Entities;

public interface IServiceEvaluationUpdateRepository
{
    void Update(ServiceEvaluation evaluation);
}

