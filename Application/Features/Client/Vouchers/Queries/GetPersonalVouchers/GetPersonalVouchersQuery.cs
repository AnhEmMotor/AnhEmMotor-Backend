using Application.ApiContracts.Voucher.Responses;
using MediatR;
using System.Collections.Generic;

using System;

namespace Application.Features.Client.Vouchers.Queries.GetPersonalVouchers;

public record GetPersonalVouchersQuery(Guid CurrentUserId) : IRequest<List<VoucherResponse>>;
