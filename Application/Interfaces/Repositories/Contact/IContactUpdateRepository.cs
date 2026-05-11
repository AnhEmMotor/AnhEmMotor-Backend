using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories.Contact
{
    public interface IContactUpdateRepository
    {
        public void Update(Domain.Entities.Contact contact);
    }
}
