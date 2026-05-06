using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.Bookings.Queries.GetBookings;

public record GetBookingsQuery : IRequest<Result<List<Booking>>>;
