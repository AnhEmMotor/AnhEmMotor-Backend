using Domain.Entities;

namespace Application.Interfaces.Repositories.Contact;

public interface IContactInsertRepository
{
    public void Add(Domain.Entities.Contact contact);
    public void AddReply(Domain.Entities.ContactReply reply);
    public void Update(Domain.Entities.Contact contact);
}
