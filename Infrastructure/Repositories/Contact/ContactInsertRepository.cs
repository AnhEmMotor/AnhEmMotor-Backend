using Application.Interfaces.Repositories.Contact;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Contact;

public class ContactInsertRepository(ApplicationDBContext context) : IContactInsertRepository
{
    public void Add(Domain.Entities.Contact contact)
    {
        context.Contacts.Add(contact);
    }

    public void AddReply(Domain.Entities.ContactReply reply)
    {
        context.ContactReplies.Add(reply);
    }

    public void Update(Domain.Entities.Contact contact)
    {
        context.Contacts.Update(contact);
    }
}
