using Application.ApiContracts.HR.Responses;
using Application.Features.HR.Commands.CreateEmployee;
using Application.Features.HR.Commands.UpdateEmployee;
using Domain.Constants.Permission;
using FluentAssertions;
using IntegrationTests.SetupClass;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationTests;

public class EmployeeCrud : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public EmployeeCrud(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task EmployeeProfile_CompletesCreateReadUpdateDeleteLifecycle()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            $"employee_admin_{suffix}",
            "AdminPass123!",
            [Permissions.Admin.EmployeeManagement.View, Permissions.Admin.EmployeeManagement.Create, Permissions.Admin.EmployeeManagement.Edit, Permissions.Admin.EmployeeManagement.Delete],
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var login = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            $"employee_admin_{suffix}",
            "AdminPass123!",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var createResponse = await HttpClientJsonExtensions.PostAsJsonAsync(
            _client,
            "/api/v1/hr/employees",
            new CreateEmployeeCommand
            {
                FullName = "Nhân viên kiểm thử",
                Email = $"employee_{suffix}@example.com",
                IdentityNumber = $"079{suffix}",
                Address = "TP.HCM",
                ContractDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                BankName = "Vietcombank",
                BankAccountNumber = $"123{suffix}",
                JobTitle = "Kỹ thuật viên",
                BaseSalary = 12_000_000m
            },
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        if (createResponse.StatusCode != HttpStatusCode.OK)
        {
            var body = await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            throw new Exception("500 Error details: " + body);
        }
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var employeeId = await createResponse.Content
            .ReadFromJsonAsync<int>(TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        var created = await _client.GetFromJsonAsync<EmployeeResponse>(
            $"/api/v1/hr/employees/{employeeId}",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        created.Should().NotBeNull();
        created!.FullName.Should().Be("Nhân viên kiểm thử");
        var updateResponse = await HttpClientJsonExtensions.PutAsJsonAsync(
            _client,
            $"/api/v1/hr/employees/{employeeId}",
            new UpdateEmployeeCommand
            {
                FullName = "Nhân viên đã cập nhật",
                Email = $"employee_updated_{suffix}@example.com",
                IdentityNumber = $"079{suffix}",
                Address = "Quận 7, TP.HCM",
                ContractDate = new DateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Utc),
                BankName = "ACB",
                BankAccountNumber = $"456{suffix}",
                JobTitle = "Trưởng nhóm kỹ thuật",
                BaseSalary = 16_000_000m
            },
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await _client.GetFromJsonAsync<EmployeeResponse>(
            $"/api/v1/hr/employees/{employeeId}",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        updated!.FullName.Should().Be("Nhân viên đã cập nhật");
        updated.Email.Should().Be($"employee_updated_{suffix}@example.com");
        updated.JobTitle.Should().Be("Trưởng nhóm kỹ thuật");
        updated.BaseSalary.Should().Be(16_000_000m);
        var deleteResponse = await _client.DeleteAsync(
            $"/api/v1/hr/employees/{employeeId}",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var remaining = await _client.GetFromJsonAsync<List<EmployeeResponse>>(
            "/api/v1/hr/employees",
            TestContext.Current.CancellationToken)
            .ConfigureAwait(true);
        remaining.Should().NotContain(employee => employee.Id == employeeId);
    }
}
