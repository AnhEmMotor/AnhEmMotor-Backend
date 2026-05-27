using AnhEmMotor.Application.Interfaces.Repositories.Lead;
using Infrastructure.DBContexts;
using System.Threading;
using System.Threading.Tasks;
using LeadEntity = global::Domain.Entities.Lead;

namespace AnhEmMotor.Infrastructure.Persistence.Repositories.Lead
{
    public class LeadInsertRepository : ILeadInsertRepository
    {
        private readonly ApplicationDBContext _db;
        public LeadInsertRepository(ApplicationDBContext db) => _db = db;

        public async Task<int> AddAsync(LeadEntity lead, CancellationToken cancellationToken)
        {
            _db.Leads.Add(lead);
            await _db.SaveChangesAsync(cancellationToken);
            return lead.Id;
        }
    }
}
