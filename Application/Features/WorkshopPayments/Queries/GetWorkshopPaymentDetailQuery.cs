using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.WorkshopPayments.Queries;

public class GetWorkshopPaymentDetailQuery : IRequest<Result<WorkshopPaymentResponse?>>
{
    public int Id { get; set; }
}
