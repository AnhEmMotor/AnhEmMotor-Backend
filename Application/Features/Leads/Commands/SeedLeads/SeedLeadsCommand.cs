using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead;
using Application.Interfaces.Repositories.User;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Leads.Commands.SeedLeads;

public record SeedLeadsCommand : IRequest<Result<bool>>;

public class SeedLeadsCommandHandler(
    ILeadWriteRepository leadWriteRepository,
    IUserReadRepository userReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SeedLeadsCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(SeedLeadsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get a sale person to assign
            var sales = await userReadRepository.GetUsersInRoleAsync("Sale", cancellationToken);
            var firstSaleId = sales.FirstOrDefault()?.Id;

            var samples = new List<Lead>
            {
                new Lead
                {
                    FullName = "Nguyễn Văn Nam",
                    Email = "nam.nguyen@gmail.com",
                    PhoneNumber = "0987123456",
                    Status = "New",
                    Source = "WebStore",
                    Score = 30,
                    InterestedVehicle = "Winner X 2024",
                    Activities = new List<LeadActivity>
                    {
                        new LeadActivity { ActivityType = "Registration", Description = "Đã đăng ký lái thử xe Winner X từ Banner ưu đãi tháng 4.", CreatedAt = DateTimeOffset.UtcNow.AddHours(-2) },
                        new LeadActivity { ActivityType = "AI_Query", Description = "Khách hỏi AI: 'Winner X có tốn xăng không?'", CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-30) }
                    }
                },
                new Lead
                {
                    FullName = "Trần Thị Mai",
                    Email = "maimai.tran@yahoo.com",
                    PhoneNumber = "0912345678",
                    Status = "Consulting",
                    Source = "Facebook",
                    Score = 65,
                    InterestedVehicle = "Air Blade 160",
                    AssignedToId = firstSaleId,
                    // Set CreatedAt/UpdatedAt to >24h ago to trigger "Slow Care" warning
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                    UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1.5),
                    Activities = new List<LeadActivity>
                    {
                        new LeadActivity { ActivityType = "Inquiry", Description = "Nhắn tin hỏi về thủ tục trả góp xe Air Blade 160.", CreatedAt = DateTimeOffset.UtcNow.AddDays(-2) },
                        new LeadActivity { ActivityType = "Call", Description = "Sale đã gọi điện tư vấn nhưng khách đang bận, hẹn gọi lại sau.", CreatedAt = DateTimeOffset.UtcNow.AddDays(-1.5) }
                    }
                },
                new Lead
                {
                    FullName = "Lê Minh Hiếu",
                    Email = "hieu.le@outlook.com",
                    PhoneNumber = "0909888999",
                    Status = "TestDriving",
                    Source = "Shop",
                    Score = 85,
                    InterestedVehicle = "CBR150R Edition",
                    AssignedToId = firstSaleId,
                    Activities = new List<LeadActivity>
                    {
                        new LeadActivity { ActivityType = "Visit", Description = "Đã đến showroom lái thử xe CBR150R.", CreatedAt = DateTimeOffset.UtcNow.AddHours(-10) },
                        new LeadActivity { ActivityType = "Process", Description = "Khách rất ưng ý, đang chờ duyệt hồ sơ vay trả góp từ ngân hàng HD Saison.", CreatedAt = DateTimeOffset.UtcNow.AddHours(-1) }
                    }
                }
            };

            foreach (var lead in samples)
            {
                leadWriteRepository.Add(lead);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Lỗi khi seed dữ liệu: {ex.Message}");
        }
    }
}
