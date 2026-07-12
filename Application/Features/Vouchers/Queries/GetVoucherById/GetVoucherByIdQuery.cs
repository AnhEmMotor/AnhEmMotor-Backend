using Application.ApiContracts.Voucher.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Vouchers.Queries.GetVoucherById;

public class GetVoucherByIdQuery : IRequest<Result<VoucherResponse>>
{
    public int Id { get; set; }

    public GetVoucherByIdQuery(int id)
    {
        Id = id;
    }
}
