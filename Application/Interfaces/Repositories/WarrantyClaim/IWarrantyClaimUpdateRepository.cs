using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.WarrantyClaim
{
    public interface IWarrantyClaimUpdateRepository
    {
        public void Add(Domain.Entities.WarrantyClaim warrantyClaim);
        public void Update(Domain.Entities.WarrantyClaim warrantyClaim);
        public void Remove(Domain.Entities.WarrantyClaim warrantyClaim);
    }
}
