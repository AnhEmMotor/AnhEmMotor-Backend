using Application.ApiContracts.Return.Responses;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Logistics.Returns.Queries.GetReturnDetail;

public class GetReturnDetailQuery : IRequest<ReturnDetailResponse?>
{
    public int Id { get; set; }
}

