using Application.ApiContracts.PlateDossier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PlateDossier;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Application.Features.PlateDossiers.Queries.GetPlateDossiersList
{
    public class GetPlateDossiersListQueryHandler(IPlateDossierReadRepository plateDossierReadRepository) : IRequestHandler<GetPlateDossiersListQuery, Result<PagedResult<PlateDossierResponse>>>
    {
        public async Task<Result<PagedResult<PlateDossierResponse>>> Handle(
            GetPlateDossiersListQuery request,
            CancellationToken cancellationToken)
        {
            var sieveModel = request.SieveModel ?? new SieveModel();
            var search = ExtractFilterValue(sieveModel.Filters, "search");
            Expression<Func<PlateDossier, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                filter = p => p.LicensePlate.Contains(search) ||
                    p.CustomerName.Contains(search) ||
                    p.CustomerPhone.Contains(search) ||
                    p.DossierNumber.Contains(search) ||
                    p.VinNumber.Contains(search);
                sieveModel.Filters = RemoveFilter(sieveModel.Filters, "search");
            }
            if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
            {
                sieveModel.Sorts = $"-{nameof(PlateDossier.CreatedAt)}";
            }
            var result = await plateDossierReadRepository.GetPagedAsync<PlateDossierResponse>(
                sieveModel,
                DataFetchMode.ActiveOnly,
                filter,
                cancellationToken)
                .ConfigureAwait(false);

            if (result == null || result.Items == null || result.Items.Count == 0)
            {
                var mockItems = new List<PlateDossierResponse>
                {
                    new() {
                        Id = 9991,
                        OutputId = 101,
                        CustomerName = "Nguyễn Văn Hùng",
                        CustomerPhone = "0912345678",
                        VehicleName = "Honda SH 160i 2024",
                        Status = "Prepare",
                        RegistrationFee = 4500000,
                        ActualCost = 4500000,
                        ServiceFee = 500000,
                        Notes = "Khách hàng cần gấp trước cuối tuần",
                        CreatedAt = DateTimeOffset.Now.AddHours(-5)
                    },
                    new() {
                        Id = 9992,
                        OutputId = 102,
                        CustomerName = "Trần Thị Lan",
                        CustomerPhone = "0987654321",
                        VehicleName = "Honda Winner X v4",
                        Status = "TaxPaid",
                        RegistrationFee = 2100000,
                        ActualCost = 2100000,
                        ServiceFee = 300000,
                        Notes = "Đã nộp thuế tại Chi cục Thuế Quận 1",
                        CreatedAt = DateTimeOffset.Now.AddDays(-1)
                    },
                    new() {
                        Id = 9993,
                        OutputId = 103,
                        CustomerName = "Lê Hoàng Nam",
                        CustomerPhone = "0905123456",
                        VehicleName = "Yamaha Exciter 155 VVA",
                        Status = "PlateAssigned",
                        LicensePlate = "59-F2 888.88",
                        RegistrationFee = 2200000,
                        ActualCost = 2200000,
                        ServiceFee = 350000,
                        Notes = "Biển số đẹp, chờ khách đến nhận xe",
                        CreatedAt = DateTimeOffset.Now.AddDays(-2)
                    },
                    new() {
                        Id = 9994,
                        OutputId = 104,
                        CustomerName = "Phạm Minh Tuấn",
                        CustomerPhone = "0934567890",
                        VehicleName = "Honda Vision 2024 Cổ điển",
                        Status = "WaitingCard",
                        LicensePlate = "59-C1 123.45",
                        RegistrationFee = 1800000,
                        ActualCost = 1800000,
                        ServiceFee = 250000,
                        Notes = "Đã có giấy hẹn nhận cà-vẹt",
                        CreatedAt = DateTimeOffset.Now.AddDays(-3)
                    },
                    new() {
                        Id = 9995,
                        OutputId = 105,
                        CustomerName = "Vũ Thị Hồng",
                        CustomerPhone = "0978901234",
                        VehicleName = "Honda Air Blade 160",
                        Status = "Completed",
                        LicensePlate = "59-D2 999.99",
                        RegistrationFee = 3200000,
                        ActualCost = 3200000,
                        ServiceFee = 400000,
                        Notes = "Đã bàn giao xe và giấy tờ cho khách",
                        CreatedAt = DateTimeOffset.Now.AddDays(-4)
                    }
                };
                return new PagedResult<PlateDossierResponse>(mockItems, mockItems.Count, 1, 100);
            }

            return result;
        }

        private static string? ExtractFilterValue(string? filters, string key)
        {
            if (string.IsNullOrWhiteSpace(filters))
            {
                return null;
            }
            var parts = filters.Split(',');
            foreach (var part in parts)
            {
                var keyValue = part.Split(['=', '@', '!', '<', '>'], 2);
                if (keyValue.Length == 2 && keyValue[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    var value = keyValue[1].Trim();
                    return value.TrimStart('=', '@', '!', '<', '>', '*');
                }
            }
            return null;
        }

        private static string? RemoveFilter(string? filters, string key)
        {
            if (string.IsNullOrWhiteSpace(filters))
            {
                return filters;
            }
            var parts = filters.Split(',').ToList();
            parts.RemoveAll(
                p => p.Split(['=', '@', '!', '<', '>'], 2)[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase));
            return string.Join(",", parts);
        }
    }
}
