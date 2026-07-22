using Application.ApiContracts.Customer.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants.Order;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Customer.Queries.GetCustomerProfile360
{
    public class GetCustomerProfile360QueryHandler(
        ILeadReadRepository leadReadRepository,
        IVehicleReadRepository vehicleReadRepository,
        IOutputReadRepository outputReadRepository,
        IMaintenanceHistoryReadRepository maintenanceHistoryReadRepository,
        IWarrantyClaimReadRepository warrantyClaimReadRepository) : IRequestHandler<GetCustomerProfile360Query, Result<CustomerProfile360Response>>
    {
        public async Task<Result<CustomerProfile360Response>> Handle(
            GetCustomerProfile360Query request,
            CancellationToken cancellationToken)
        {
            var lead = await leadReadRepository.GetByIdAsync(request.LeadId, cancellationToken).ConfigureAwait(false);
            if (lead == null)
            {
                return Result<CustomerProfile360Response>.Failure("Không tìm thấy khách hàng.");
            }

            var vehicles = await vehicleReadRepository.GetByLeadIdAsync(request.LeadId, cancellationToken).ConfigureAwait(false);
            var outputs = await outputReadRepository.GetByLeadIdAsync(request.LeadId, cancellationToken).ConfigureAwait(false);

            var vehiclesList = new List<OwnedVehicleInfo>();
            var maintenanceList = new List<MaintenanceHistorySummary>();
            var warrantyList = new List<WarrantyClaimSummary>();

            foreach (var v in vehicles)
            {
                vehiclesList.Add(new OwnedVehicleInfo
                {
                    Id = v.Id,
                    VinNumber = v.VinNumber,
                    LicensePlate = v.LicensePlate,
                    EngineNumber = v.EngineNumber,
                    VariantName = v.ProductVariant?.VariantName ?? "Winner X 2024", // Fallback for display
                    ColorName = v.ProductVariantColor?.ColorName ?? "Đỏ Đen",
                    PurchaseDate = v.PurchaseDate,
                    Status = v.Status.ToString(),
                    CurrentOdo = (int)v.CurrentOdo
                });

                // Get maintenance history for each vehicle
                var maints = await maintenanceHistoryReadRepository.GetByVehicleIdAsync(v.Id, cancellationToken).ConfigureAwait(false);
                foreach (var m in maints)
                {
                    maintenanceList.Add(new MaintenanceHistorySummary
                    {
                        Id = m.Id,
                        MaintenanceNumber = m.MaintenanceNumber,
                        VehicleId = m.VehicleId,
                        LicensePlate = v.LicensePlate,
                        VariantName = v.ProductVariant?.VariantName ?? "Winner X 2024",
                        MaintenanceDate = m.MaintenanceDate,
                        Description = m.Description,
                        Mileage = m.Mileage,
                        PartsCost = m.PartsCost,
                        LaborCost = m.LaborCost,
                        TotalCost = m.TotalCost,
                        NextMaintenanceDate = m.NextMaintenanceDate
                    });
                }

                // Get warranty claims for each vehicle
                var claims = await warrantyClaimReadRepository.GetByVehicleIdAsync(v.Id, cancellationToken).ConfigureAwait(false);
                foreach (var c in claims)
                {
                    warrantyList.Add(new WarrantyClaimSummary
                    {
                        Id = c.Id,
                        ClaimNumber = c.ClaimNumber,
                        StatusText = Domain.Constants.WarrantyClaimStatus.GetLabel(c.Status),
                        CreatedAt = c.CreatedAt ?? DateTimeOffset.MinValue
                    });
                }
            }

            var outputsList = new List<InvoiceSummary>();
            foreach (var o in outputs)
            {
                var items = new List<InvoiceItemSummary>();
                if (o.OutputInfos != null)
                {
                    foreach (var oi in o.OutputInfos)
                    {
                        items.Add(new InvoiceItemSummary
                        {
                            Id = oi.Id,
                            ProductName = oi.ProductVariant?.VariantName ?? "Phụ tùng / Khác",
                            Count = oi.Count,
                            Price = oi.Price,
                            CoverImageUrl = oi.ProductVariant?.CoverImageUrl
                        });
                    }
                }

                outputsList.Add(new InvoiceSummary
                {
                    Id = o.Id,
                    StatusId = o.StatusId,
                    StatusDisplayName = OrderStatus.GetDisplayName(o.StatusId ?? string.Empty),
                    Total = o.Total,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,
                    CreatedAt = o.CreatedAt ?? DateTimeOffset.MinValue,
                    LastStatusChangedAt = o.LastStatusChangedAt,
                    Items = items
                });
            }

            // Create timeline events
            var timelineEvents = new List<TimelineEventResponse>();

            // 1. Output events
            foreach (var o in outputsList)
            {
                timelineEvents.Add(new TimelineEventResponse
                {
                    Date = o.CreatedAt.ToString("o"),
                    Type = "output_created",
                    Title = $"Tạo đơn hàng #{o.Id}",
                    Description = $"Tổng tiền: {o.Total:N0}đ · Trạng thái: {o.StatusDisplayName}",
                    Status = o.StatusId,
                    RelatedId = o.Id
                });

                if (o.LastStatusChangedAt.HasValue)
                {
                    timelineEvents.Add(new TimelineEventResponse
                    {
                        Date = o.LastStatusChangedAt.Value.ToString("o"),
                        Type = "output_status",
                        Title = $"Đơn hàng #{o.Id} đổi trạng thái",
                        Description = $"Trạng thái mới: {o.StatusDisplayName}",
                        Status = o.StatusId,
                        RelatedId = o.Id
                    });
                }
            }

            // 2. Maintenance events
            foreach (var m in maintenanceList)
            {
                timelineEvents.Add(new TimelineEventResponse
                {
                    Date = m.MaintenanceDate.ToString("o"),
                    Type = "service",
                    Title = $"Bảo dưỡng #{m.MaintenanceNumber}",
                    Description = $"{m.VariantName} ({m.LicensePlate}) · ODO: {m.Mileage:N0} km · Chi phí: {m.TotalCost:N0}đ",
                    RelatedId = m.Id
                });
            }

            // 3. Activity / Note events (Excluding AI chat events)
            if (lead.Activities != null)
            {
                foreach (var a in lead.Activities.Where(act => act.ActivityType.ToLower() == "note"))
                {
                    timelineEvents.Add(new TimelineEventResponse
                    {
                        Date = (a.CreatedAt ?? DateTimeOffset.MinValue).ToString("o"),
                        Type = "activity",
                        Title = "Ghi chú nội bộ",
                        Description = a.Description,
                        RelatedId = a.Id
                    });
                }
            }

            // Sort timeline events by date descending
            timelineEvents = timelineEvents.OrderByDescending(t => t.Date).ToList();

            // Create care reminders
            var careReminders = new List<CareReminderResponse>();
            if (lead.Birthday.HasValue)
            {
                var nextBday = new DateTime(DateTime.Today.Year, lead.Birthday.Value.Month, lead.Birthday.Value.Day);
                if (nextBday < DateTime.Today) nextBday = nextBday.AddYears(1);
                
                careReminders.Add(new CareReminderResponse
                {
                    Type = "birthday",
                    Title = $"Sinh nhật khách hàng ({lead.Birthday.Value:dd/MM})",
                    Description = $"Chúc mừng sinh nhật khách hàng {lead.FullName} vào ngày {lead.Birthday.Value:dd/MM}.",
                    DueDate = new DateTimeOffset(nextBday, TimeSpan.Zero),
                    Priority = "normal"
                });
            }

            // Stalled order reminder
            var pendingOutputs = outputsList.Where(o => o.StatusId == OrderStatus.Pending || o.StatusId == OrderStatus.WaitingDeposit).ToList();
            foreach (var po in pendingOutputs)
            {
                if ((DateTimeOffset.UtcNow - po.CreatedAt).TotalDays > 3)
                {
                    careReminders.Add(new CareReminderResponse
                    {
                        Type = "stalled_order",
                        Title = $"Đơn hàng #{po.Id} chưa thanh toán/xác nhận",
                        Description = $"Đơn hàng #{po.Id} của khách hàng đã ở trạng thái {po.StatusDisplayName} hơn 3 ngày.",
                        DueDate = po.CreatedAt.AddDays(3),
                        Priority = "high"
                    });
                }
            }

            var summary = new Profile360SummaryResponse
            {
                OwnedVehiclesCount = vehiclesList.Count,
                ActiveOutputsCount = outputsList.Count(o => o.StatusId != OrderStatus.Completed && o.StatusId != OrderStatus.Cancelled && o.StatusId != OrderStatus.Refunded),
                OverdueRemindersCount = careReminders.Count(r => r.Priority == "high" || r.Priority == "urgent")
            };

            return Result<CustomerProfile360Response>.Success(new CustomerProfile360Response
            {
                Id = lead.Id,
                FullName = lead.FullName,
                PhoneNumber = lead.PhoneNumber,
                Email = lead.Email,
                Address = lead.Address,
                AddressDetail = lead.AddressDetail,
                Ward = lead.Ward,
                Province = lead.Province,
                Gender = lead.Gender,
                Birthday = lead.Birthday,
                IdentificationNumber = lead.IdentificationNumber,
                CreatedAt = lead.CreatedAt ?? DateTimeOffset.MinValue,
                IsVerified = lead.IsVerified,
                Tier = lead.Tier,
                Points = lead.Points,
                InterestedVehicle = lead.InterestedVehicle,
                AssignedToId = lead.AssignedToId,
                AssignedToName = lead.AssignedTo?.FullName,
                Vehicles = vehiclesList,
                Outputs = outputsList,
                MaintenanceHistories = maintenanceList,
                WarrantyClaims = warrantyList,
                TimelineEvents = timelineEvents,
                CareReminders = careReminders,
                Summary = summary
            });
        }
    }
}
