using Application.ApiContracts.Leads.Responses;
using Application.Features.Bookings.Commands.ConfirmBooking;
using Application.Features.Bookings.Commands.CreateBooking;
using Domain.Constants.Booking;
using Domain.Constants.Lead;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationTests;

public class Lead : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public Lead(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _output = output;
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(true);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "LEAD_001 - Lấy danh sách Lead và các hoạt động đi kèm")]
    public async Task GetLeads_ReturnsLeadsWithActivities()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [PermissionsList.Leads.View],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead1 = new Domain.Entities.Lead
        {
            FullName = "Lead 1",
            PhoneNumber = "0901000001",
            Status = LeadStatus.Consulting,
            Score = 30
        };
        var lead2 = new Domain.Entities.Lead
        {
            FullName = "Lead 2",
            PhoneNumber = "0901000002",
            Status = LeadStatus.Consulting,
            Score = 30
        };
        db.Leads.AddRange(lead1, lead2);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        db.LeadActivities
            .AddRange(
                new LeadActivity
                {
                    LeadId = lead1.Id,
                    ActivityType = LeadActivityType.Booking,
                    Description = "Activity 1",
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new LeadActivity
                {
                    LeadId = lead2.Id,
                    ActivityType = LeadActivityType.Booking,
                    Description = "Activity 2",
                    CreatedAt = DateTimeOffset.UtcNow
                });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var response = await _client.GetAsync("/api/v1/lead", CancellationToken.None).ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<List<LeadResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content.Should().HaveCountGreaterThanOrEqualTo(2);
        content.Any(l => string.Compare(l.PhoneNumber, "0901000001") == 0 && l.Activities.Count > 0).Should().BeTrue();
    }

    [Fact(DisplayName = "LEAD_005 - Chuyển đổi trạng thái Lead khi xác nhận lịch lái thử")]
    public async Task ConfirmBooking_UpdatesLeadStatusToTestDriving()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [PermissionsList.Bookings.Confirm],
            CancellationToken.None,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead = new Domain.Entities.Lead
        {
            FullName = "Booking Lead",
            PhoneNumber = "0905000001",
            Status = LeadStatus.Consulting,
            Score = 30
        };
        db.Leads.Add(lead);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var booking = new Domain.Entities.Booking
        {
            FullName = lead.FullName,
            PhoneNumber = lead.PhoneNumber,
            Email = "lead@gmail.com",
            Status = BookingStatus.Pending,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            Location = "Showroom A",
            BookingType = BookingType.TestDrive
        };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var confirmCommand = new ConfirmBookingCommand { BookingId = booking.Id };
        var response = await _client.PostAsJsonAsync("/api/v1/bookings/confirm", confirmCommand).ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedLead = await db.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lead.Id, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updatedLead.Should().NotBeNull();
        updatedLead!.Status.Should().Be(LeadStatus.TestDriving);
        var activities = await db.LeadActivities
            .Where(a => a.LeadId == lead.Id)
            .ToListAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        activities.Should()
            .Contain(
                a => string.Compare(a.ActivityType, LeadActivityType.Contact) == 0 &&
                    a.Description.Contains("Đang lái thử"));
    }

    [Fact(DisplayName = "LEAD_017 - Ghi nhận hoạt động đặt lịch cho khách hàng mới")]
    public async Task CreateBooking_NewCustomer_CreatesActivityWithNewTag()
    {
        var phoneNumber = "0909000170";
        var command = new CreateBookingCommand
        {
            PhoneNumber = phoneNumber,
            FullName = "New Customer",
            BookingType = "Consulting",
            Location = "Online"
        };
        await _client.PostAsJsonAsync("/api/v1/bookings", command).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var activity = await db.LeadActivities
            .Include(a => a.Lead)
            .FirstOrDefaultAsync(
                a => string.Compare(a.Lead.PhoneNumber, phoneNumber) == 0,
                TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        activity.Should().NotBeNull();
        activity!.Description.Should().Contain("(Khách hàng mới)");
    }

    [Fact(DisplayName = "LEAD_018 - Ghi nhận hoạt động đặt lịch cho khách hàng cũ")]
    public async Task CreateBooking_ExistingCustomer_CreatesActivityWithoutNewTag()
    {
        var phoneNumber = "0909000180";
        using (var setupScope = _factory.Services.CreateScope())
        {
            var setupDb = setupScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            setupDb.Leads.Add(new Domain.Entities.Lead { FullName = "Old", PhoneNumber = phoneNumber });
            await setupDb.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var command = new CreateBookingCommand
        {
            PhoneNumber = phoneNumber,
            FullName = "Existing",
            BookingType = "Consulting",
            Location = "Online"
        };
        await _client.PostAsJsonAsync("/api/v1/bookings", command).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var activity = await db.LeadActivities
            .Include(a => a.Lead)
            .Where(a => a.Lead.PhoneNumber == phoneNumber)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        activity.Should().NotBeNull();
        activity!.Description.Should().NotContain("(Khách hàng mới)");
    }

    [Fact(DisplayName = "LEAD_019 - Tự động tạo hoạt động khi xác nhận lịch hẹn lái thử")]
    public async Task ConfirmBooking_ValidRequest_CreatesContactActivity()
    {
        var phoneNumber = "0909000190";
        int bookingId;
        using (var setupScope = _factory.Services.CreateScope())
        {
            var setupDb = setupScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var lead = new Domain.Entities.Lead { FullName = "Test", PhoneNumber = phoneNumber };
            setupDb.Leads.Add(lead);
            var booking = new Domain.Entities.Booking
            {
                FullName = "Test",
                PhoneNumber = phoneNumber,
                Email = "test@gmail.com",
                Status = BookingStatus.Pending,
                PreferredDate = DateTime.UtcNow,
                Location = "Showroom",
                BookingType = BookingType.TestDrive
            };
            setupDb.Bookings.Add(booking);
            await setupDb.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            bookingId = booking.Id;
        }
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_{uniqueId}",
            "Password123!",
            [],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var confirmCommand = new ConfirmBookingCommand { BookingId = bookingId };
        await _client.PostAsJsonAsync("/api/v1/bookings/confirm", confirmCommand).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var activity = await db.LeadActivities
            .Include(a => a.Lead)
            .Where(a => a.Lead.PhoneNumber == phoneNumber && a.ActivityType == LeadActivityType.Contact)
            .FirstOrDefaultAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        activity.Should().NotBeNull();
        activity!.Description.Should().Contain("Đang lái thử");
    }

    [Fact(DisplayName = "LEAD_025 - Kiểm tra liên kết khóa ngoại giữa Lead và Activity")]
    public async Task GetLead_WithActivities_ReturnsCorrectCount()
    {
        var phoneNumber = "0909000250";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var lead = new Domain.Entities.Lead { FullName = "User 25", PhoneNumber = phoneNumber };
            db.Leads.Add(lead);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            db.LeadActivities
                .AddRange(
                    new LeadActivity { LeadId = lead.Id, ActivityType = "Note", Description = "A1" },
                    new LeadActivity { LeadId = lead.Id, ActivityType = "Note", Description = "A2" },
                    new LeadActivity { LeadId = lead.Id, ActivityType = "Note", Description = "A3" });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var leadWithActivities = await db.Leads
                .Include(l => l.Activities)
                .FirstOrDefaultAsync(
                    l => string.Compare(l.PhoneNumber, phoneNumber) == 0,
                    TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            leadWithActivities.Should().NotBeNull();
            leadWithActivities!.Activities.Should().HaveCount(3);
        }
    }

    [Fact(DisplayName = "LEAD_029 - Tích lũy điểm qua nhiều lần đặt lịch liên tiếp")]
    public async Task CreateBooking_ThreeTimes_ScoreReaches90()
    {
        var phoneNumber = "0909000300";
        var command = new CreateBookingCommand
        {
            PhoneNumber = phoneNumber,
            FullName = "Multi Booking User",
            BookingType = "Consulting",
            Location = "Online"
        };
        await _client.PostAsJsonAsync("/api/v1/bookings", command).ConfigureAwait(true);
        await _client.PostAsJsonAsync("/api/v1/bookings", command).ConfigureAwait(true);
        await _client.PostAsJsonAsync("/api/v1/bookings", command).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead = await db.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(
                l => string.Compare(l.PhoneNumber, phoneNumber) == 0,
                TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        lead.Should().NotBeNull();
        lead!.Score.Should().Be(90);
    }

    [Fact(DisplayName = "LEAD_033 - Bảo toàn điểm số khi thông tin cá nhân thay đổi")]
    public async Task CreateBooking_SamePhoneNewName_ScoreIncreasesAndNameUpdates()
    {
        var phoneNumber = "0909000700";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Leads.Add(new Domain.Entities.Lead { FullName = "Old Name", PhoneNumber = phoneNumber, Score = 30 });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var command = new CreateBookingCommand
        {
            PhoneNumber = phoneNumber,
            FullName = "New Name",
            BookingType = "Consulting",
            Location = "Online"
        };
        await _client.PostAsJsonAsync("/api/v1/bookings", command).ConfigureAwait(true);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var lead = await db.Leads
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    l => string.Compare(l.PhoneNumber, phoneNumber) == 0,
                    TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            lead.Should().NotBeNull();
            lead!.Score.Should().Be(60);
        }
    }

    [Fact(DisplayName = "LEAD_035 - Xác minh lưu trữ Score xuống Database")]
    public async Task CreateBooking_SavesScoreToDatabase()
    {
        var phoneNumber = "0909000900";
        var command = new CreateBookingCommand
        {
            PhoneNumber = phoneNumber,
            FullName = "Database User",
            BookingType = "Consulting",
            Location = "Online"
        };
        var response = await _client.PostAsJsonAsync("/api/v1/bookings", command).ConfigureAwait(true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead = await db.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(
                l => string.Compare(l.PhoneNumber, phoneNumber) == 0,
                TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        lead.Should().NotBeNull();
        lead!.Score.Should().Be(30);
    }
}
