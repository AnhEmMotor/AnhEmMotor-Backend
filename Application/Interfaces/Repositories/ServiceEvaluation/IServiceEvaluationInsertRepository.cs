namespace Application.Interfaces.Repositories.ServiceEvaluation;

using Domain.Entities;

public interface IServiceEvaluationInsertRepository
{
    void Add(ServiceEvaluation evaluation);
}

