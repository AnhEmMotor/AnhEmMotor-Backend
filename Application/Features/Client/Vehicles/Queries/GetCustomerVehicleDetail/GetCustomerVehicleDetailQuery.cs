using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.Client.Vehicles.Queries.GetCustomerVehicleDetail;

public class GetCustomerVehicleDetailQuery : IRequest<Result<VehicleDetailResponse>>
{
    public Guid UserId { get; set; }

    public int VehicleId { get; set; }
}
