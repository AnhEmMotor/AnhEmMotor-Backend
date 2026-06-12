namespace Application.Interfaces.Repositories.PlateDossier
{
    public interface IPlateDossierUpdateRepository
    {
        void Add(Domain.Entities.PlateDossier plateDossier);
        void Update(Domain.Entities.PlateDossier plateDossier);
        void Remove(Domain.Entities.PlateDossier plateDossier);
    }
}
