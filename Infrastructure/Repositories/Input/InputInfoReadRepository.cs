using Application.Interfaces.Repositories.Input;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Input
{
    public class InputInfoReadRepository(ApplicationDBContext context) : IInputInfoReadRepository
    {
        public Task<List<InputInfo>> GetFinishedInputInfosByVariantIdAsync(int variantId, CancellationToken cancellationToken = default)
        {
            return context.InputInfos
                .Include(ii => ii.InputReceipt)
                .Where(ii => ii.ProductId == variantId && ii.InputReceipt != null && ii.InputReceipt.StatusId == "finished")
                .ToListAsync(cancellationToken);
        }
    }
}
