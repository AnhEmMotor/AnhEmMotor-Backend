namespace Application.Interfaces.Repositories.BookingAppointment;

public interface IBookingAppointmentWriteRepository
{
	public void Add(global::Domain.Entities.BookingAppointment entity);

	public void Update(global::Domain.Entities.BookingAppointment entity);

	public void Delete(global::Domain.Entities.BookingAppointment entity);
}
