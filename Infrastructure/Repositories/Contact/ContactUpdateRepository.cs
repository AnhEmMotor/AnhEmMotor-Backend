using Application.Interfaces.Repositories.Contact;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Contact
{
    public class ContactUpdateRepository(ApplicationDBContext context): IContactUpdateRepository
    {
        public void Update(Domain.Entities.Contact contact)
        {
            context.Contacts.Update(contact);
        }
    }
}
