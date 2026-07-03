using Application.Interfaces.Repositories.WarrantyClaim;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.WarrantyClaim
{
    public class WarrantyClaimUpdateRepository(ApplicationDBContext context) : IWarrantyClaimUpdateRepository
    {
        public void Add(Domain.Entities.WarrantyClaim warrantyClaim)
        {
            context.WarrantyClaims.Add(warrantyClaim);
        }

        public void Update(Domain.Entities.WarrantyClaim warrantyClaim)
        {
            context.WarrantyClaims.Update(warrantyClaim);
        }

        public void Remove(Domain.Entities.WarrantyClaim warrantyClaim)
        {
            context.WarrantyClaims.Remove(warrantyClaim);
        }
    }
}
