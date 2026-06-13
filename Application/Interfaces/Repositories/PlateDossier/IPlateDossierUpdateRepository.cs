
namespace Application.Interfaces.Repositories.PlateDossier
{
    public interface IPlateDossierUpdateRepository
    {
        public void Add(Domain.Entities.PlateDossier plateDossier);

        public void Update(Domain.Entities.PlateDossier plateDossier);

        public void Remove(Domain.Entities.PlateDossier plateDossier);
    }
}
