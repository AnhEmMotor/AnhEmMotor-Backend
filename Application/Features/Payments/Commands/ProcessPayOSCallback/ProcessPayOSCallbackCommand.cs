using Application.Common.Models;
using MediatR;

namespace Application.Features.Payments.Commands.ProcessPayOSCallback;

public sealed record ProcessPayOSCallbackCommand(long? OrderCode) : IRequest<Result<int>>
{
    public int OrderId
    {
        get
        {
            if (OrderCode is null) return 0;
            var id = (int)(OrderCode.Value / 100000);
            return id == 0 ? (int)OrderCode.Value : id;
        }
    }
}
