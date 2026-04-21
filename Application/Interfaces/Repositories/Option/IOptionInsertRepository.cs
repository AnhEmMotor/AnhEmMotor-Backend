using OptionEntity = Domain.Entities.Option;

namespace Application.Interfaces.Repositories.Option;

public interface IOptionInsertRepository
{
    public void Add(OptionEntity option);
}
