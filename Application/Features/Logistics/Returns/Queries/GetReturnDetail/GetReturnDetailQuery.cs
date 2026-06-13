using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.ApiContracts.Return.Responses;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Entities.Logistics;
using MediatR;

namespace Application.Features.Logistics.Returns.Queries.GetReturnDetail;

public class GetReturnDetailQuery : IRequest<ReturnDetailResponse?>
{
    public int Id { get; set; }
}


