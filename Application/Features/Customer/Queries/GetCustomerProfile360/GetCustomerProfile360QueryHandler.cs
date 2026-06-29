using Application.ApiContracts.Customer.Responses;
using Application.ApiContracts.Leads.Responses;
using Application.ApiContracts.PlateDossier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.PlateDossier;
using Application.Interfaces.Repositories.RepairOrder;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants.Order;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System;
using System.Linq;

namespace Application.Features.Customer.Queries.GetCustomerProfile360;

public class GetCustomerProfile360QueryHandler(
  ILeadReadRepository leadReadRepository,
  IOutputReadRepository outputReadRepository,
  IPlateDossierReadRepository plateDossierReadRepository,
  IRepairOrderReadRepository repairOrderReadRepository,
  IVehicleReadRepository vehicleReadRepository) : IRequestHandler<GetCustomerProfile360Query, Result<CustomerProfile360Response>>
{
  public async Task<Result<CustomerProfile360Response>> Handle(GetCustomerProfile360Query request, CancellationToken cancellationToken)
  {
    var lead = await leadReadRepository.GetByIdAsync(request.LeadId, cancellationToken).ConfigureAwait(false);
    if (lead == null)
      return Result<CustomerProfile360Response>.Failure("Không tìm thấy khách hàng.");

    var response = new CustomerProfile360Response
    {
      Id = lead.Id,
      FullName = lead.FullName,
      Email = lead.Email,
      PhoneNumber = lead.PhoneNumber,
      Score = lead.Score,
      Status = lead.Status,
      Source = lead.Source,
      InterestedVehicle = lead.InterestedVehicle,
      Address = lead.Address,
      AddressDetail = lead.AddressDetail,
      Ward = lead.Ward,
      District = lead.District,
      Province = lead.Province,
      Gender = lead.Gender,
      Birthday = lead.Birthday,
      IdentificationNumber = lead.IdentificationNumber,
      CreatedAt = lead.CreatedAt,
      IsVerified = lead.IsVerified,
      Tier = lead.Tier,
      Points = lead.Points,
      AssignedToId = lead.AssignedToId,
      AssignedToName = lead.AssignedTo?.FullName,
      Activities = lead.Activities?
        .Select(a => new LeadActivityResponse
        {
          Id = a.Id,
          ActivityType = a.ActivityType,
          Description = a.Description,
          CreatedAt = a.CreatedAt ?? DateTimeOffset.MinValue
        })
        .OrderByDescending(a => a.CreatedAt)
        .ToList() ?? []
    };

    // Load related outputs
    var outputs = await outputReadRepository.GetByLeadIdAsync(request.LeadId, cancellationToken).ConfigureAwait(false);
    var outputIds = outputs.Select(o => o.Id).ToArray();

    // Load plate dossiers for these outputs
    var plateDossiers = outputIds.Length > 0
      ? await plateDossierReadRepository.GetPagedAsync<PlateDossierResponse>(
          new SieveModel { PageSize = int.MaxValue },
          filter: pd => outputIds.Contains(pd.OutputId),
          cancellationToken: cancellationToken).ConfigureAwait(false)
      : new PagedResult<PlateDossierResponse>(null!, 0, 0, 0);

    // Load repair orders by customer phone
    var repairOrders = !string.IsNullOrWhiteSpace(lead.PhoneNumber)
      ? await repairOrderReadRepository.GetByCustomerPhoneAsync(lead.PhoneNumber, cancellationToken).ConfigureAwait(false)
      : [];

    // Load vehicles by LeadId
    var vehicles = await vehicleReadRepository.GetByLeadIdAsync(request.LeadId, cancellationToken).ConfigureAwait(false);

    // Map outputs
    var plateMap = plateDossiers.Items?.ToDictionary(pd => pd.OutputId) ?? new Dictionary<int, PlateDossierResponse>();

    response.Outputs = outputs.Select(o =>
    {
      var pd = plateMap.GetValueOrDefault(o.Id);
      return new CustomerOutputSummary
      {
        Id = o.Id,
        StatusId = o.StatusId,
        StatusDisplayName = OrderStatus.GetDisplayName(o.StatusId ?? string.Empty),
      CreatedAt = o.CreatedAt,
        LastStatusChangedAt = o.LastStatusChangedAt,
        Total = o.Total,
        PaymentMethod = o.PaymentMethod,
        PaymentStatus = o.PaymentStatus,
        Items = o.OutputInfos?.Where(oi => oi.DeletedAt == null).Select(oi => new CustomerOutputItemSummary
        {
          Id = oi.Id,
          ProductName = oi.ProductVariant?.Product?.Name,
          Count = oi.Count,
          Price = oi.Price,
          CoverImageUrl = oi.ProductVariant?.ProductCollectionPhotos?.FirstOrDefault()?.ImageUrl
        }).ToList() ?? []
      };
    }).ToList();

    // Map vehicles
    response.Vehicles = vehicles.Select(v => new CustomerVehicleSummary
    {
      Id = v.Id,
      LicensePlate = v.LicensePlate,
      VinNumber = v.VinNumber,
      EngineNumber = v.EngineNumber,
      PurchaseDate = v.PurchaseDate,
      Status = v.Status,
      LastMaintenanceDate = v.LastMaintenanceDate,
      NextMaintenanceDate = v.NextMaintenanceDate,
      NextMaintenanceOdo = v.NextMaintenanceOdo,
      CurrentOdo = v.CurrentOdo
    }).ToList();

    // Build care reminders
    var reminders = new List<CustomerCareReminder>();

    // Birthday reminder
    if (lead.Birthday.HasValue)
    {
      var nextBirthday = new DateTime(DateTime.UtcNow.Year, lead.Birthday.Value.Month, lead.Birthday.Value.Day);
      if (nextBirthday < DateTime.UtcNow)
        nextBirthday = nextBirthday.AddYears(1);
      var daysUntil = (nextBirthday - DateTime.UtcNow).TotalDays;
      reminders.Add(new CustomerCareReminder
      {
        Type = "birthday",
        Title = "Sinh nhật khách hàng",
        Description = $"{lead.FullName} - {nextBirthday:dd/MM/yyyy}",
        DueDate = nextBirthday,
        Priority = daysUntil <= 7 ? "high" : daysUntil <= 30 ? "normal" : "low"
      });
    }

    // Maintenance reminders from vehicles
    foreach (var v in vehicles)
    {
      if (v.NextMaintenanceDate.HasValue)
      {
        var daysUntil = (v.NextMaintenanceDate.Value - DateTime.UtcNow).TotalDays;
        reminders.Add(new CustomerCareReminder
        {
          Type = "maintenance",
          Title = $"Bảo dưỡng xe {v.LicensePlate ?? v.VinNumber}",
          Description = $"ODO: {v.NextMaintenanceOdo ?? 0} km - Dự kiến: {v.NextMaintenanceDate.Value:dd/MM/yyyy}",
          DueDate = v.NextMaintenanceDate,
          Priority = daysUntil <= 0 ? "urgent" : daysUntil <= 7 ? "high" : daysUntil <= 30 ? "normal" : "low"
        });
      }
    }

    // Stalled transaction reminders
    var stalledThreshold = DateTimeOffset.UtcNow.AddDays(-30);
    foreach (var o in outputs)
    {
      if (o.LastStatusChangedAt.HasValue && o.LastStatusChangedAt < stalledThreshold
        && !IsTerminalOutput(o.StatusId))
      {
        reminders.Add(new CustomerCareReminder
        {
          Type = "stalled_order",
          Title = $"Đơn hàng #{o.Id} kẹt trạng thái",
          Description = $"Trạng thái: {OrderStatus.GetDisplayName(o.StatusId ?? string.Empty)} - Đã thay đổi: {o.LastStatusChangedAt.Value:dd/MM/yyyy}",
          DueDate = DateTime.UtcNow.Date,
          Priority = "high"
        });
      }
    }

    response.CareReminders = reminders
      .OrderBy(r => r.DueDate ?? DateTimeOffset.MaxValue)
      .ThenByDescending(r => RankReminderPriority(r.DueDate))
      .ToList();

    // Build timeline events
    var timelineEvents = new List<CustomerTimelineEvent>();

    // Output status changes
    foreach (var o in outputs)
    {
      timelineEvents.Add(new CustomerTimelineEvent
      {
        Date = o.CreatedAt ?? DateTimeOffset.MinValue,
        Type = "output_created",
        Title = $"Đơn hàng #{o.Id} được tạo",
        Description = $"Tổng: {o.Total:N0}đ",
        Status = o.StatusId,
        RelatedId = o.Id
      });

      if (o.LastStatusChangedAt.HasValue)
      {
        timelineEvents.Add(new CustomerTimelineEvent
        {
          Date = o.LastStatusChangedAt.Value,
          Type = "output_status",
          Title = $"Đơn hàng #{o.Id}: {OrderStatus.GetDisplayName(o.StatusId ?? string.Empty)}",
          Status = o.StatusId,
          RelatedId = o.Id
        });
      }
    }

    // Plate dossier events
    foreach (var pd in plateDossiers.Items ?? Enumerable.Empty<PlateDossierResponse>())
    {
      timelineEvents.Add(new CustomerTimelineEvent
      {
        Date = pd.CreatedAt,
        Type = "plate_dossier",
        Title = $"Hồ sơ đăng ký xe #{pd.Id}",
        Description = pd.LicensePlate != null ? $"Biển số: {pd.LicensePlate}" : null,
        Status = pd.Status,
        RelatedId = pd.OutputId
      });
    }

    // Repair order events
    foreach (var ro in repairOrders)
    {
      timelineEvents.Add(new CustomerTimelineEvent
      {
        Date = ro.CreatedAt ?? DateTimeOffset.MinValue,
        Type = "service",
        Title = $"Phiếu sửa chữa #{ro.Id}",
        Description = ro.Description,
        Status = ro.Status,
        RelatedId = ro.Id
      });
    }

    // Lead activities
    foreach (var a in lead.Activities ?? Enumerable.Empty<LeadActivity>())
    {
      timelineEvents.Add(new CustomerTimelineEvent
      {
        Date = a.CreatedAt ?? DateTimeOffset.MinValue,
        Type = "activity",
        Title = $"{a.ActivityType}: {a.Description}",
        RelatedId = a.Id
      });
    }

    response.TimelineEvents = timelineEvents
      .OrderByDescending(e => e.Date)
      .ToList();

    // Summary
    var terminalStatuses = new[] { OrderStatus.Completed, OrderStatus.Cancelled, OrderStatus.Refunded };
    response.Summary = new Customer360Summary
    {
      ActiveOutputsCount = outputs.Count(o => !terminalStatuses.Contains(o.StatusId ?? string.Empty)),
      OwnedVehiclesCount = vehicles.Count(v => v.Status == "Sold"),
      OverdueRemindersCount = reminders.Count(r => r.Priority is "urgent" or "high"),
      LastInteractionDate = lead.Activities?
        .OrderByDescending(a => a.CreatedAt)
        .FirstOrDefault()?.CreatedAt
    };

    return Result<CustomerProfile360Response>.Success(response);
  }

  private static int RankReminderPriority(DateTime? dueDate)
  {
    if (!dueDate.HasValue)
      return 0;

    var daysUntil = (dueDate.Value.Date - DateTime.UtcNow.Date).TotalDays;
    return daysUntil <= 0 ? 4 : daysUntil <= 7 ? 3 : daysUntil <= 30 ? 2 : 1;
  }

  private static bool IsTerminalOutput(string? statusId) =>
    string.Equals(statusId, OrderStatus.Completed, StringComparison.OrdinalIgnoreCase)
    || string.Equals(statusId, OrderStatus.Cancelled, StringComparison.OrdinalIgnoreCase)
    || string.Equals(statusId, OrderStatus.Refunded, StringComparison.OrdinalIgnoreCase);
}
