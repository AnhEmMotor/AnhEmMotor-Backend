using System;

namespace Application.Interfaces.Repositories.Contact
{
    public interface IContactUpdateRepository
    {
        public void Update(Domain.Entities.Contact contact);
    }
}
