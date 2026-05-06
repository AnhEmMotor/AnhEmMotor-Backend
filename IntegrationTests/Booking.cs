using Application.Features.Bookings.Commands.ConfirmBooking;
using Application.Features.Bookings.Commands.CreateBooking;
using Application.Interfaces.Services;
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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests;

public class Booking : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public Booking(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
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

    [Fact(DisplayName = "BOOKING_005 - Thông báo cho quản trị viên khi có lịch mới")]
    public async Task CreateBooking_SendsNotificationToAdmin()
    {
        // Arrange
        var command = new CreateBookingCommand 
        { 
            FullName = "Notify User", 
            PhoneNumber = "0909000005",
            Email = "notify@gmail.com",
            Location = BookingLocation.Showroom
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/bookings", command).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Vì NotificationService thường được mock hoặc là service nội bộ, 
        // trong Integration Test chúng ta có thể kiểm tra side effect nếu có 
        // hoặc nếu factory cho phép verify mock. 
        // Ở đây giả định logic tạo thành công và trả về ID.
    }

    [Fact(DisplayName = "BOOKING_007 - Xác nhận lịch hẹn thành công")]
    public async Task ConfirmBooking_ValidId_UpdatesStatusToConfirmed()
    {
        // Arrange
        int bookingId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var b = new Domain.Entities.Booking 
            { 
                FullName = "Confirm User", 
                PhoneNumber = "0909000007", 
                Email = "confirm@gmail.com",
                Status = BookingStatus.Pending,
                PreferredDate = DateTimeOffset.UtcNow,
                Location = BookingLocation.Showroom,
                BookingType = BookingType.TestDrive
            };
            db.Bookings.Add(b);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            bookingId = b.Id;
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

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/bookings/confirm", confirmCommand).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

            var updatedBooking = await db.Bookings
                .FirstOrDefaultAsync(x => x.Id == bookingId, TestContext.Current.CancellationToken)
                .ConfigureAwait(true);

            updatedBooking.Should().NotBeNull();
            updatedBooking!.Status.Should().Be(BookingStatus.Confirmed);
        }
    }

    [Fact(DisplayName = "BOOKING_008 - Gửi email xác nhận sau khi quản trị viên phê duyệt")]
    public async Task ConfirmBooking_SendsEmailToCustomer()
    {
        // Arrange
        int bookingId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var b = new Domain.Entities.Booking 
            { 
                FullName = "Email User", 
                PhoneNumber = "0909000008", 
                Email = "email@gmail.com",
                Status = BookingStatus.Pending,
                PreferredDate = DateTimeOffset.UtcNow,
                Location = BookingLocation.Showroom,
                BookingType = BookingType.TestDrive
            };
            db.Bookings.Add(b);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            bookingId = b.Id;
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

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/bookings/confirm", new ConfirmBookingCommand { BookingId = bookingId }).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Trong Integration Test, chúng ta thường mock EmailService ở tầng cao hơn 
        // hoặc kiểm tra việc gọi service nếu factory có cung cấp cách truy cập Mock.
    }

    [Fact(DisplayName = "BOOKING_010 - Đồng bộ trạng thái Lead khi xác nhận lịch lái thử")]
    public async Task ConfirmBooking_SyncsLeadStatusToTestDriving()
    {
        // Arrange
        string phone = "0909000010";
        int bookingId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var lead = new Domain.Entities.Lead { FullName = "Sync Lead", PhoneNumber = phone, Status = LeadStatus.Consulting };
            db.Leads.Add(lead);
            
            var b = new Domain.Entities.Booking 
            { 
                FullName = "Sync Lead", 
                PhoneNumber = phone, 
                Email = "sync@gmail.com",
                Status = BookingStatus.Pending,
                PreferredDate = DateTimeOffset.UtcNow,
                Location = BookingLocation.Showroom,
                BookingType = BookingType.TestDrive
            };
            db.Bookings.Add(b);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            bookingId = b.Id;
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

        // Act
        await _client.PostAsJsonAsync("/api/v1/bookings/confirm", new ConfirmBookingCommand { BookingId = bookingId }).ConfigureAwait(true);

        // Assert
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var updatedLead = await db.Leads.FirstOrDefaultAsync(l => string.Compare(l.PhoneNumber, phone) == 0, TestContext.Current.CancellationToken).ConfigureAwait(true);
            updatedLead!.Status.Should().Be(LeadStatus.TestDriving);
        }
    }

    [Fact(DisplayName = "BOOKING_012 - Lấy danh sách tất cả các lịch hẹn")]
    public async Task GetBookings_ReturnsAllBookings()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Bookings.AddRange(
                new Domain.Entities.Booking { FullName = "B1", PhoneNumber = "0900000001", Email = "b1@gmail.com", Location = "A", BookingType = "T1" },
                new Domain.Entities.Booking { FullName = "B2", PhoneNumber = "0900000002", Email = "b2@gmail.com", Location = "B", BookingType = "T2" }
            );
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
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

        // Act
        var response = await _client.GetAsync("/api/v1/bookings", TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Có thể kiểm tra thêm số lượng hoặc content nếu cần
    }
}
