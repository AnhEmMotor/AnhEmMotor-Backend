using Application.ApiContracts.Voucher.Responses;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Client.Vouchers.Queries.GetPersonalVouchers;

public record GetPersonalVouchersQuery : IRequest<List<VoucherResponse>>;
