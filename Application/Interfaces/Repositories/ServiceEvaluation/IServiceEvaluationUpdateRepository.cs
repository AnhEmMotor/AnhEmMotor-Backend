namespace Application.Interfaces.Repositories.ServiceEvaluation;

using Domain.Entities;

public interface IServiceEvaluationUpdateRepository
{
    public void Update(ServiceEvaluation evaluation);
}

