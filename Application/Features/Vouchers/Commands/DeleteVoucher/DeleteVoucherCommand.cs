using Application.Common.Models;
using MediatR;

namespace Application.Features.Vouchers.Commands.DeleteVoucher;

public class DeleteVoucherCommand : IRequest<Result<int>>
{
    public int Id { get; set; }

    public DeleteVoucherCommand(int id)
    {
        Id = id;
    }
}
