namespace Application.Interfaces.Repositories.ServiceEvaluation;

using Domain.Entities;

public interface IServiceEvaluationInsertRepository
{
    public void Add(ServiceEvaluation evaluation);
}

