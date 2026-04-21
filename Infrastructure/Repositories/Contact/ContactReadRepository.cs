using Application.Interfaces.Repositories.Contact;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Contact;

public class ContactReadRepository(ApplicationDBContext context) : IContactReadRepository
{
    public async Task<Domain.Entities.Contact?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Contacts
            .Include(c => c.Replies)
                .ThenInclude(r => r.RepliedBy)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Domain.Entities.Contact>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Contacts
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
