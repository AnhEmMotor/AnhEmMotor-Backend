using Application.Interfaces.Repositories.PlateDossier;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.PlateDossier
{
    public class PlateDossierUpdateRepository(ApplicationDBContext context) : IPlateDossierUpdateRepository
    {
        public void Add(Domain.Entities.PlateDossier plateDossier)
        {
            context.PlateDossiers.Add(plateDossier);
        }

        public void Update(Domain.Entities.PlateDossier plateDossier)
        {
            context.PlateDossiers.Update(plateDossier);
        }

        public void Remove(Domain.Entities.PlateDossier plateDossier)
        {
            context.PlateDossiers.Remove(plateDossier);
        }
    }
}
