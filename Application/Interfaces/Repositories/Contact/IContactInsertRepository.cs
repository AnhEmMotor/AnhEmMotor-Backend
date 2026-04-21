using Domain.Entities;

namespace Application.Interfaces.Repositories.Contact;

public interface IContactInsertRepository
{
    void Add(Domain.Entities.Contact contact);
    void AddReply(Domain.Entities.ContactReply reply);
    void Update(Domain.Entities.Contact contact);
}
