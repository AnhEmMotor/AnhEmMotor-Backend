
namespace Application.Interfaces.Repositories.Lead;

public interface ILeadInsertRepository
{
    public void Add(Domain.Entities.Lead lead);

    public void Update(Domain.Entities.Lead lead);
}
