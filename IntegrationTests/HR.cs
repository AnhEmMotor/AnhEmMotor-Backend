using Application.Features.HR.Commands.CreateCommissionPolicy;
using Application.Features.HR.Commands.CreateEmployee;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using OutputEntity = Domain.Entities.Output;
using ProductEntity = Domain.Entities.Product;

namespace IntegrationTests;

public class HR : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public HR(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "HR01 - Tạo mới hồ sơ nhân viên thành công")]
    public async Task HR01_Create_Employee_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            "admin",
            "AdminPass123!",
            [Domain.Constants.Permission.Permissions.Admin.EmployeeManagement.Create],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var adminLogin = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            "admin",
            "AdminPass123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.AccessToken);
        var command = new CreateEmployeeCommand
        {
            FullName = "Test Staff",
            Email = $"staff_{uniqueId}@example.com",
            IdentityNumber = "001200012345",
            Address = "123 Main St",
            ContractDate = DateTime.UtcNow,
            BankName = "Vietcombank",
            BankAccountNumber = "1234567890",
            JobTitle = "Sales Consultant",
            BaseSalary = 10000000
        };
        var response = await HttpClientJsonExtensions.PostAsJsonAsync(
            _client,
            "/api/v1/hr/employees",
            command,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var employee = await db.EmployeeProfiles
            .FirstOrDefaultAsync(
                e => string.Compare(e.IdentityNumber, "001200012345") == 0,
                TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        employee.Should().NotBeNull();
        employee!.BaseSalary.Should().Be(10000000);
    }

    [Fact(DisplayName = "HR04 - Ngăn chặn tạo chính sách hoa hồng trùng lặp thời gian")]
    public async Task HR04_Prevent_Overlapping_CommissionPolicy()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var admin = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"admin_hr04_prev_{uniqueId}",
            "Password123!",
            [],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"admin_hr04_prev_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var prod = new ProductEntity { Name = $"Test Product HR04_{uniqueId}", StatusId = "for-sale" };
        db.Products.Add(prod);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var productId = prod.Id;
        var command1 = new CreateCommissionPolicyCommand
        {
            Name = $"Policy 1 {uniqueId}",
            ProductId = productId,
            Value = 500000,
            Type = "FixedAmount",
            EffectiveDate = DateTimeOffset.UtcNow.AddDays(-10),
            IsActive = true,
            CurrentUserId = admin.Id,
            CurrentUserName = "Admin"
        };
        await HttpClientJsonExtensions.PostAsJsonAsync(
            _client,
            "/api/v1/hr/commission-policies",
            command1,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var command2 = new CreateCommissionPolicyCommand
        {
            Name = $"Overlapping Policy {uniqueId}",
            ProductId = productId,
            Value = 600000,
            Type = "FixedAmount",
            EffectiveDate = DateTimeOffset.UtcNow.AddDays(-5),
            IsActive = true,
            CurrentUserId = admin.Id,
            CurrentUserName = "Admin"
        };
        var response = await HttpClientJsonExtensions.PostAsJsonAsync(
            _client,
            "/api/v1/hr/commission-policies",
            command2,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        if (response!.StatusCode == HttpStatusCode.InternalServerError)
        {
            var error = await response!.Content
                .ReadAsStringAsync(TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            throw new Exception($"500 Error in HR04_Overlapping: {error}");
        }
        response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "HR04 - Cho phép tạo chính sách hoa hồng tiếp nối thời gian")]
    public async Task HR04_Allow_Adjacent_CommissionPolicy()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var prod = new ProductEntity { Name = $"Test Product HR04-2_{uniqueId}", StatusId = "for-sale" };
        db.Products.Add(prod);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var endDate = DateTimeOffset.UtcNow.AddDays(10);
        var admin = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"admin_hr04_{uniqueId}",
            "Password123!",
            [],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"admin_hr04_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var command1 = new CreateCommissionPolicyCommand
        {
            Name = $"Base Policy {uniqueId}",
            Type = "Percentage",
            Value = 5,
            ProductId = null,
            CategoryId = null,
            EffectiveDate = DateTimeOffset.UtcNow.AddDays(-10),
            IsActive = true,
            CurrentUserId = admin.Id,
            CurrentUserName = "Admin"
        };
        await HttpClientJsonExtensions.PostAsJsonAsync(
            _client,
            "/api/v1/hr/commission-policies",
            command1,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var command2 = new CreateCommissionPolicyCommand
        {
            Name = $"New Policy {uniqueId}",
            ProductId = prod.Id,
            Value = 600000,
            Type = "FixedAmount",
            EffectiveDate = endDate.AddSeconds(1),
            IsActive = true,
            CurrentUserId = admin.Id,
            CurrentUserName = "Admin"
        };
        var response = await HttpClientJsonExtensions.PostAsJsonAsync(
            _client,
            "/api/v1/hr/commission-policies",
            command2,
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        if (response!.StatusCode == HttpStatusCode.InternalServerError)
        {
            var error = await response!.Content
                .ReadAsStringAsync(TestContext.Current.CancellationToken)
                .ConfigureAwait(true);
            throw new Exception($"500 Error in HR04: {error}");
        }
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "HR07 - Cập nhật trạng thái thanh toán bảng lương")]
    public async Task HR07_Update_Payroll_Status_Paid()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _ = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"admin_hr07_{uniqueId}",
            "Password123!",
            [Domain.Constants.Permission.Permissions.Admin.EmployeeManagement.Edit, Domain.Constants.Permission.Permissions.Admin.EmployeeManagement.View, Domain.Constants.Permission.Permissions.Admin.PayrollManagement.Approve, Domain.Constants.Permission.Permissions.Admin.PayrollManagement.View],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"admin_hr07_{uniqueId}",
            "Password123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var user = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"user_hr07_{uniqueId}",
            "Password123!",
            [],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var emp = new EmployeeProfile { UserId = user.Id, BaseSalary = 10000000 };
        db.EmployeeProfiles.Add(emp);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var status = await db.OutputStatuses
            .FirstOrDefaultAsync(s => string.Compare(s.Key, "completed") == 0, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        if (status == null)
        {
            status = new OutputStatus { Key = "completed" };
            db.OutputStatuses.Add(status);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }
        var output = new OutputEntity { CustomerName = "Test", PaidAmount = 1000, StatusId = "completed" };
        db.OutputOrders.Add(output);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var now = DateTime.UtcNow;
        var record = new CommissionRecord
        {
            EmployeeProfileId = emp.Id,
            OutputId = output.Id,
            Amount = 500000,
            Status = CommissionStatus.Confirmed,
            DateEarned = now
        };
        db.CommissionRecords.Add(record);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        var response = await HttpClientJsonExtensions.PostAsJsonAsync(
            _client,
            "/api/v1/hr/commissions/approve-payroll",
            new { month = now.Month, year = now.Year },
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updatedRecord = await db.CommissionRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == record.Id, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updatedRecord!.Status.Should().Be(CommissionStatus.Paid);
        updatedRecord.PaidAt.Should().NotBeNull();
    }
}

