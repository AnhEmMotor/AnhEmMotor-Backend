using Application.ApiContracts.Leads.Responses;
using Application.Features.Bookings.Commands.ConfirmBooking;
using Application.Features.Bookings.Commands.CreateBooking;
using Application.Features.Leads.Commands.AddLeadActivity;
using Application.Features.Leads.Commands.UpdateLead;
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
        await _factory.ResetDatabaseAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
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
            [Domain.Constants.Permission.Permissions.Leads.View],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
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
        var response = await _client.GetAsync("/api/v1/lead", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<List<LeadResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Should().HaveCountGreaterThanOrEqualTo(2);
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
            [Domain.Constants.Permission.Permissions.Bookings.Confirm],
            TestContext.Current.CancellationToken,
            email)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            TestContext.Current.CancellationToken)
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
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
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
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"user_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var confirmCommand = new ConfirmBookingCommand { BookingId = bookingId };
        await _client.PostAsJsonAsync("/api/v1/bookings/confirm", confirmCommand).ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var activity = await db.LeadActivities
            .Include(a => a.Lead)
            .Where(
                a => a.Lead.PhoneNumber == phoneNumber && string.Compare(a.ActivityType, LeadActivityType.Contact) == 0)
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
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
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

    [Fact(DisplayName = "LEAD_046 - Cộng điểm khi khách hàng lái thử")]
    public async Task LEAD_046_TestDrive_Increases_Score()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _ = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"sales_{uniqueId}",
            "Password123!",
            [Domain.Constants.Permission.Permissions.Leads.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"sales_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead = new Domain.Entities.Lead { FullName = "Test Drive Lead", PhoneNumber = $"09{uniqueId}", Score = 0 };
        db.Leads.Add(lead);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var command = new AddLeadActivityCommand(lead.Id, "TestDrive", "Khách hàng lái thử xe");
        var response = await _client.PostAsJsonAsync($"/api/v1/lead/{lead.Id}/activities", command).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedLead = await db.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lead.Id, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updatedLead!.Score.Should().Be(20);
    }

    [Fact(DisplayName = "LEAD_047 - Cộng điểm khi hỏi về trả góp")]
    public async Task LEAD_047_InstallmentInquiry_Increases_Score()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _ = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"sales_{uniqueId}",
            "Password123!",
            [Domain.Constants.Permission.Permissions.Leads.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"sales_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead = new Domain.Entities.Lead { FullName = "Installment Lead", PhoneNumber = $"09{uniqueId}", Score = 20 };
        db.Leads.Add(lead);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var command = new AddLeadActivityCommand(lead.Id, "Consulting", "Khách hỏi về trả góp (installment)");
        var response = await _client.PostAsJsonAsync($"/api/v1/lead/{lead.Id}/activities", command).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedLead = await db.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lead.Id, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updatedLead!.Score.Should().Be(50);
    }

    [Fact(DisplayName = "LEAD_048 - Trừ điểm khi có cuộc gọi lỡ")]
    public async Task LEAD_048_MissedCall_Decreases_Score()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _ = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"sales_{uniqueId}",
            "Password123!",
            [Domain.Constants.Permission.Permissions.Leads.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"sales_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead = new Domain.Entities.Lead { FullName = "Missed Call Lead", PhoneNumber = $"09{uniqueId}", Score = 50 };
        db.Leads.Add(lead);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var command = new AddLeadActivityCommand(lead.Id, "Call", "Missed call");
        var response = await _client.PostAsJsonAsync($"/api/v1/lead/{lead.Id}/activities", command).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedLead = await db.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lead.Id, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updatedLead!.Score.Should().Be(40);
    }

    [Fact(DisplayName = "LEAD_051 - Điều chỉnh điểm thủ công bởi Admin")]
    public async Task LEAD_051_Admin_Adjustment_Score()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _ = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"admin_{uniqueId}",
            "Password123!",
            [Domain.Constants.Permission.Permissions.Leads.Edit],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"admin_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead = new Domain.Entities.Lead { FullName = "Adjustment Lead", PhoneNumber = $"09{uniqueId}", Score = 10 };
        db.Leads.Add(lead);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var command = new UpdateLeadCommand
        {
            Id = lead.Id,
            FullName = lead.FullName,
            PhoneNumber = lead.PhoneNumber,
            Score = 100
        };
        var response = await _client.PutAsJsonAsync($"/api/v1/lead/{lead.Id}", command).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedLead = await db.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lead.Id, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updatedLead!.Score.Should().Be(100);
    }

    [Fact(DisplayName = "LEAD_037 - Tạo Lead với đầy đủ hồ sơ định danh")]
    public async Task LEAD_037_Create_Lead_Full_Profile_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var payload = new
        {
            FullName = $"Lead {uniqueId}",
            PhoneNumber = $"08{uniqueId}",
            IdentificationNumber = $"CCCD{uniqueId}",
            Birthday = DateTime.Now.AddYears(-25),
            Gender = "Male"
        };
        _ = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"admin_lead_039_{uniqueId}",
            "Password123!",
            [Domain.Constants.Permission.Permissions.Leads.View, Domain.Constants.Permission.Permissions.Leads.Create, Domain.Constants.Permission.Permissions.Leads.Edit, Domain.Constants.Permission.Permissions.Leads.Delete],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"admin_lead_039_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var response = await _client.PostAsJsonAsync("/api/v1/lead", payload).ConfigureAwait(true);
        if (response!.StatusCode == HttpStatusCode.Created)
        {
            var resultId = await response!.Content
                .ReadFromJsonAsync<int>(TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            resultId.Should().BeGreaterThan(0);
        }
    }

    [Fact(DisplayName = "LEAD_039.1 - Kiểm tra ràng buộc trùng số CCCD (Scenario 1)")]
    public async Task LEAD_039_Duplicate_IdentificationNumber_Returns_BadRequest_1()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var cccd = $"CCCD{uniqueId}";
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        db.Leads
            .Add(
                new Domain.Entities.Lead
                {
                    FullName = "Existing",
                    PhoneNumber = "0900000001",
                    IdentificationNumber = cccd
                });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var payload = new { FullName = "New Lead", PhoneNumber = "0900000002", IdentificationNumber = cccd };
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            "admin_lead_dup",
            "Password123!",
            [Domain.Constants.Permission.Permissions.Leads.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            "admin_lead_dup",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var response = await _client.PostAsJsonAsync("/api/v1/lead", payload).ConfigureAwait(true);
        if (response!.StatusCode != HttpStatusCode.NotFound)
        {
            response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact(DisplayName = "LEAD_039.2 - Kiểm tra ràng buộc trùng số CCCD (Scenario 2)")]
    public async Task LEAD_039_Duplicate_IdentificationNumber_Returns_BadRequest_2()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var cccd = $"CCCD{uniqueId}";
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        db.Leads
            .Add(
                new Domain.Entities.Lead
                {
                    FullName = "Lead A",
                    PhoneNumber = "0911111111",
                    IdentificationNumber = cccd
                });
        var leadB = new Domain.Entities.Lead
        {
            FullName = "Lead B",
            PhoneNumber = "0922222222",
            IdentificationNumber = "OTHER"
        };
        db.Leads.Add(leadB);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"admin_lead_039_2_{uniqueId}",
            "Password123!",
            [Domain.Constants.Permission.Permissions.Leads.Edit],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"admin_lead_039_2_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var command = new UpdateLeadCommand { Id = leadB.Id, FullName = "Lead B", IdentificationNumber = cccd };
        var response = await _client.PutAsJsonAsync($"/api/v1/lead/{leadB.Id}", command).ConfigureAwait(true);
        if (response!.StatusCode != HttpStatusCode.NotFound)
        {
            response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact(DisplayName = "LEAD_040 - Phân công Lead cho nhân viên kinh doanh")]
    public async Task LEAD_040_Assign_Lead_To_Staff_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead = new Domain.Entities.Lead { FullName = "Unassigned Lead", PhoneNumber = $"07{uniqueId}" };
        db.Leads.Add(lead);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"admin_lead_040_{uniqueId}",
            "Password123!",
            [Domain.Constants.Permission.Permissions.Leads.Edit],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var adminLogin = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"admin_lead_040_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.AccessToken);
        var staffUser = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"staff_lead_040_{uniqueId}",
            "Password123!",
            [],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var salesId = staffUser.Id;
        var response = await _client.PostAsJsonAsync($"/api/v1/lead/{lead.Id}/assign", (Guid?)salesId)
            .ConfigureAwait(true);
        response!.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "LEAD_042 - Truy vấn danh sách Lead theo xe quan tâm")]
    public async Task LEAD_042_Filter_Leads_By_Vehicle_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var vehicle = "Honda SH 150i";
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        db.Leads
            .Add(
                new Domain.Entities.Lead
                {
                    FullName = "Interested User",
                    PhoneNumber = "0988888888",
                    InterestedVehicle = vehicle
                });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"admin_lead_042_{uniqueId}",
            "Password123!",
            [Domain.Constants.Permission.Permissions.Leads.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var adminLogin = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"admin_lead_042_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.AccessToken);
        var response = await _client.GetAsync(
            $"/api/v1/lead?InterestedVehicle={Uri.EscapeDataString(vehicle)}",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<List<LeadResponse>>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
    }

    [Fact(DisplayName = "LEAD_044 - Xóa mềm (Soft Delete) Lead")]
    public async Task LEAD_044_Soft_Delete_Lead_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var lead = new Domain.Entities.Lead { FullName = "To Be Deleted", PhoneNumber = $"06{uniqueId}" };
        db.Leads.Add(lead);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            "admin",
            "Password123!",
            [Domain.Constants.Permission.Permissions.Leads.Delete],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var adminLogin = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            "admin",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.AccessToken);
        var response = await _client.DeleteAsync($"/api/v1/lead/{lead.Id}", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        if (response!.StatusCode != HttpStatusCode.NotFound && response!.StatusCode != HttpStatusCode.MethodNotAllowed)
        {
            response!.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var deletedLead = await db.Leads
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == lead.Id, TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            deletedLead!.DeletedAt.Should().NotBeNull();
        }
    }
}


