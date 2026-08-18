using Application.ApiContracts.Voucher.Responses;
using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Repositories.Voucher;
using Domain.Enums;
using MediatR;
using Sieve.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Client.Vouchers.Queries.GetPersonalVouchers;

public class GetPersonalVouchersQueryHandler : IRequestHandler<GetPersonalVouchersQuery, List<VoucherResponse>>
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly ILeadReadRepository _leadReadRepository;
    private readonly IVoucherReadRepository _voucherReadRepository;

    public GetPersonalVouchersQueryHandler(
        IUserReadRepository userReadRepository,
        ILeadReadRepository leadReadRepository,
        IVoucherReadRepository voucherReadRepository)
    {
        _userReadRepository = userReadRepository;
        _leadReadRepository = leadReadRepository;
        _voucherReadRepository = voucherReadRepository;
    }

    public async Task<List<VoucherResponse>> Handle(GetPersonalVouchersQuery request, CancellationToken cancellationToken)
    {
        var user = await _userReadRepository.FindUserByIdAsync(request.CurrentUserId, cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.Email))
            return new List<VoucherResponse>();

        var leads = await _leadReadRepository.GetPagedAsync<Domain.Entities.Lead>(
            new SieveModel { PageSize = 1, Page = 1 },
            Domain.Constants.DataFetchMode.ActiveOnly,
            l => l.Email == user.Email,
            cancellationToken);

        var lead = leads.Items.FirstOrDefault();
        if (lead == null)
            return new List<VoucherResponse>();

        var vouchersPage = await _voucherReadRepository.GetPagedAsync<VoucherResponse>(
            new SieveModel { PageSize = 100, Page = 1, Sorts = "ValidTo" },
            Domain.Constants.DataFetchMode.ActiveOnly,
            v => v.VoucherLeads.Any(vl => vl.LeadId == lead.Id) && v.ValidTo >= DateTime.UtcNow.Date,
            cancellationToken);

        return vouchersPage.Items.ToList();
    }
}
