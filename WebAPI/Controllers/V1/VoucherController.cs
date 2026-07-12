using Application.ApiContracts.Voucher.Requests;
using Application.Features.Vouchers.Commands.CreateVoucher;
using Application.Features.Vouchers.Commands.DeleteVoucher;
using Application.Features.Vouchers.Commands.UpdateVoucher;
using Application.Features.Vouchers.Queries.GetVoucherById;
using Application.Features.Vouchers.Queries.GetVoucherList;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class VoucherController(IMediator mediator) : ApiController
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetVouchers([FromQuery] GetVouchersRequest request)
    {
        var result = await mediator.Send(new GetVouchersQuery { Request = request });
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetVoucherById(int id)
    {
        var result = await mediator.Send(new GetVoucherByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherRequest request)
    {
        var result = await mediator.Send(new CreateVoucherCommand { Request = request });
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateVoucher(int id, [FromBody] UpdateVoucherRequest request)
    {
        if (id != request.Id)
            return BadRequest("Id không hợp lệ");
        var result = await mediator.Send(new UpdateVoucherCommand { Request = request });
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteVoucher(int id)
    {
        var result = await mediator.Send(new DeleteVoucherCommand(id));
        return Ok(result);
    }
}
