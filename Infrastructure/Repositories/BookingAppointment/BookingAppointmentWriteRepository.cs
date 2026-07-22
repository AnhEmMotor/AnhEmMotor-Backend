using Application.Interfaces.Repositories.BookingAppointment;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.BookingAppointment;

public class BookingAppointmentWriteRepository(ApplicationDBContext context) : IBookingAppointmentWriteRepository
{
    public void Add(global::Domain.Entities.BookingAppointment entity) => context.Set<global::Domain.Entities.BookingAppointment>(
        )
        .Add(entity);

    public void Update(global::Domain.Entities.BookingAppointment entity) => context.Set<global::Domain.Entities.BookingAppointment>(
        )
        .Update(entity);

    public void Delete(global::Domain.Entities.BookingAppointment entity) => context.Set<global::Domain.Entities.BookingAppointment>(
        )
        .Remove(entity);
}
