using Application.ApiContracts.Input.Requests;
using Application.ApiContracts.Input.Responses;
using Application.Features.Inputs.Commands.CreateInput;
using Application.Features.Inputs.Commands.DeleteManyInputs;
using Application.Features.Inputs.Commands.RestoreManyInputs;
using Application.Features.Inputs.Commands.UpdateInput;
using Application.Features.Inputs.Commands.UpdateInputStatus;
using Application.Features.Inputs.Commands.UpdateManyInputStatus;
using Azure.Core;
using Domain.Constants.Permission;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace IntegrationTests;

public class InventoryReceipts : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public InventoryReceipts(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "INPUT_001 - Tạo phiếu nhập hàng thành công (Happy Path)")]
    public async Task CreateInput_Success_ReturnsOk()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var request = new UpdateInputCommand
        {
            Notes = "Nhập hàng tháng 1",
            SupplierId = 1,
            Products = [ new UpdateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/InventoryReceipts");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(request);
        var response = await _client.SendAsync(requestMessage);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Id.Should().NotBeNull();
        content.StatusId.Should().Be(Domain.Constants.Input.InputStatus.Working);
        content.Products.Should().HaveCount(1);
        content.Products[0].Count.Should().Be(10);
        content.Products[0].InputPrice.Should().Be(100000);
        content.TotalPayable.Should().Be(1000000);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = db.InputReceipts.FirstOrDefault(i => i.Id == content.Id);
        input.Should().NotBeNull();
        input!.StatusId.Should().Be(Domain.Constants.Input.InputStatus.Working);
    }

    [Fact(DisplayName = "INPUT_002 - Tạo phiếu nhập với nhiều sản phẩm và tính toán chính xác tổng tiền")]
    public async Task CreateInput_MultipleProducts_CalculatesTotalCorrectly()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products =
                [ new CreateInputInfoRequest { ProductId = 1, Count = 5, InputPrice = 123456.78m }, new CreateInputInfoRequest
                {
                    ProductId = 2,
                    Count = 3,
                    InputPrice = 987654.32m
                } ]
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/InventoryReceipts");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(request);
        var response = await _client.SendAsync(requestMessage);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();

        decimal expectedTotal = (5 * 123456.78m) + (3 * 987654.32m);
        content!.TotalPayable.Should().Be((long)expectedTotal);
    }

    [Fact(DisplayName = "INPUT_004 - Tạo phiếu nhập với SupplierId không tồn tại")]
    public async Task CreateInput_NonExistentSupplier_ReturnsNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 9999,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/InventoryReceipts");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(request);
        var response = await _client.SendAsync(requestMessage);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_005 - Tạo phiếu nhập với ProductId không tồn tại")]
    public async Task CreateInput_NonExistentProduct_ReturnsNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 9999, Count = 10, InputPrice = 100000 } ]
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/InventoryReceipts");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(request);
        var response = await _client.SendAsync(requestMessage);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_006 - Tạo phiếu nhập với Product đã bị xóa hoặc không còn bán")]
    public async Task CreateInput_DeletedProduct_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };


        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/InventoryReceipts");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(request);
        var response = await _client.SendAsync(requestMessage);


        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_012 - Tạo phiếu nhập với Notes chứa script XSS")]
    public async Task CreateInput_XSSInNotes_SanitizesInput()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var request = new CreateInputCommand
        {
            Notes = "<script>alert('XSS')</script>",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/InventoryReceipts");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(request);
        var response = await _client.SendAsync(requestMessage);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = db.InputReceipts.FirstOrDefault(i => i.Id == content!.Id);
        input!.Notes.Should().NotContain("<script>");
    }

    [Fact(DisplayName = "INPUT_016 - Lấy danh sách phiếu nhập có phân trang")]
    public async Task GetInputs_WithPagination_ReturnsCorrectPage()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v1/InventoryReceipts?page=1&pageSize=10");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<InputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().HaveCountLessThanOrEqualTo(10);
        content.PageNumber.Should().Be(1);
    }

    [Fact(DisplayName = "INPUT_017 - Lấy danh sách phiếu nhập với filter theo StatusId")]
    public async Task GetInputs_FilterByStatus_ReturnsFilteredResults()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v1/InventoryReceipts?filters=StatusId==working");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<InputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items
            .Should()
            .OnlyContain(i => string.Compare(i.StatusId, Domain.Constants.Input.InputStatus.Working) == 0);
    }

    [Fact(DisplayName = "INPUT_018 - Lấy danh sách phiếu nhập với sort theo InputDate descending")]
    public async Task GetInputs_SortByInputDateDesc_ReturnsSortedResults()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v1/InventoryReceipts?sorts=-InputDate");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<InputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().BeInDescendingOrder(i => i.InputDate);
    }

    [Fact(DisplayName = "INPUT_020 - Lấy chi tiết phiếu nhập thành công")]
    public async Task GetInputById_ExistingInput_ReturnsCompleteDetails()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        int inputId = 1;

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v1/InventoryReceipts?sorts=-InputDate");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Id.Should().Be(inputId);
        content.StatusId.Should().NotBeNullOrEmpty();
        content.Products.Should().NotBeNull();
        content.SupplierName.Should().NotBeNullOrEmpty();
        content.TotalPayable.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact(DisplayName = "INPUT_021 - Lấy chi tiết phiếu nhập không tồn tại")]
    public async Task GetInputById_NonExistentInput_ReturnsNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        int inputId = 9999;

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/InventoryReceipts/{inputId}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.SendAsync(requestMessage);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "INPUT_023 - Cập nhật phiếu nhập thành công ở trạng thái working")]
    public async Task UpdateInput_WorkingStatus_UpdatesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var createRequest = new CreateInputCommand
        {
            Notes = "Original",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/InventoryReceipts");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestMessage.Content = JsonContent.Create(createRequest);
        var createResponse = await _client.SendAsync(requestMessage);
        var createdInput = await createResponse.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        var updateRequest = new UpdateInputCommand
        {
            Notes = "Updated",
            SupplierId = 2,
            Products = [ new UpdateInputInfoRequest { ProductId = 2, Count = 20, InputPrice = 200000 } ]
        };

        var requestUpdateMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/InventoryReceipts/{createdInput!.Id}");
        requestUpdateMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        requestUpdateMessage.Content = JsonContent.Create(updateRequest);
        var response = await _client.SendAsync(requestMessage);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Notes.Should().Be("Updated");
        content.SupplierId.Should().Be(2);
        content.TotalPayable.Should().Be(4000000);
    }

    [Fact(DisplayName = "INPUT_024 - Cập nhật phiếu nhập ở trạng thái finished (không cho phép)")]
    public async Task UpdateInput_FinishedStatus_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var createRequest = new CreateInputCommand
        {
            Notes = "Original",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest)
            .ConfigureAwait(true);
        var createdInput = await createResponse.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        await _client.PatchAsJsonAsync(
            $"/api/v1/InventoryReceipts/{createdInput!.Id}/status",
            new UpdateInputStatusCommand { StatusId = Domain.Constants.Input.InputStatus.Finish },
            CancellationToken.None)
            .ConfigureAwait(true);

        var updateRequest = new UpdateInputCommand { Notes = "Updated", SupplierId = 2, Products = [] };

        var response = await _client.PutAsJsonAsync($"/api/v1/InventoryReceipts/{createdInput.Id}", updateRequest)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_025 - Cập nhật phiếu nhập ở trạng thái cancelled (không cho phép)")]
    public async Task UpdateInput_CancelledStatus_ReturnsBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var createRequest = new CreateInputCommand
        {
            Notes = "Original",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest)
            .ConfigureAwait(true);
        var createdInput = await createResponse.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        await _client.PatchAsJsonAsync(
            $"/api/v1/InventoryReceipts/{createdInput!.Id}/status",
            new UpdateInputStatusCommand { StatusId = Domain.Constants.Input.InputStatus.Cancel },
            CancellationToken.None)
            .ConfigureAwait(true);

        var updateRequest = new UpdateInputCommand { Notes = "Updated", SupplierId = 2, Products = [] };

        var response = await _client.PutAsJsonAsync($"/api/v1/InventoryReceipts/{createdInput.Id}", updateRequest)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_028 - Cập nhật trạng thái phiếu nhập từ working sang finished")]
    public async Task UpdateInputStatus_WorkingToFinished_UpdatesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var createRequest = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest)
            .ConfigureAwait(true);
        var createdInput = await createResponse.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        var statusRequest = new UpdateInputStatusCommand { StatusId = Domain.Constants.Input.InputStatus.Finish };

        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/InventoryReceipts/{createdInput!.Id}/status",
            statusRequest,
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.StatusId.Should().Be(Domain.Constants.Input.InputStatus.Finish);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = db.InputReceipts.FirstOrDefault(i => i.Id == createdInput.Id);
        input!.StatusId.Should().Be(Domain.Constants.Input.InputStatus.Finish);
        input.ConfirmedBy.Should().NotBeNull();
    }

    [Fact(DisplayName = "INPUT_029 - Cập nhật trạng thái phiếu nhập từ working sang cancelled")]
    public async Task UpdateInputStatus_WorkingToCancelled_UpdatesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var createRequest = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest)
            .ConfigureAwait(true);
        var createdInput = await createResponse.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        var statusRequest = new UpdateInputStatusCommand { StatusId = Domain.Constants.Input.InputStatus.Cancel };

        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/InventoryReceipts/{createdInput!.Id}/status",
            statusRequest,
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.StatusId.Should().Be(Domain.Constants.Input.InputStatus.Cancel);
    }

    [Fact(DisplayName = "INPUT_032 - Cập nhật trạng thái nhiều phiếu nhập cùng lúc")]
    public async Task UpdateManyInputStatus_MultipleInputs_UpdatesAllSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var ids = new List<int>();
        for(int i = 0; i < 3; i++)
        {
            var createRequest = new CreateInputCommand
            {
                Notes = $"Test {i}",
                SupplierId = 1,
                Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
            };
            var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest)
                .ConfigureAwait(true);
            var createdInput = await createResponse.Content
                .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
                .ConfigureAwait(true);
            ids.Add(createdInput!.Id!.Value);
        }

        var statusRequest = new UpdateManyInputStatusCommand
        {
            Ids = ids,
            StatusId = Domain.Constants.Input.InputStatus.Finish
        };

        var response = await _client.PatchAsJsonAsync(
            "/api/v1/InventoryReceipts/status",
            statusRequest,
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        foreach(var id in ids)
        {
            var input = db.InputReceipts.FirstOrDefault(i => i.Id == id);
            input!.StatusId.Should().Be(Domain.Constants.Input.InputStatus.Finish);
        }
    }

    [Fact(DisplayName = "INPUT_033 - Cập nhật trạng thái nhiều phiếu nhập với một số Id không tồn tại")]
    public async Task UpdateManyInputStatus_SomeNonExistent_HandlesPartially()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var createRequest = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest)
            .ConfigureAwait(true);
        var createdInput = await createResponse.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        var statusRequest = new UpdateManyInputStatusCommand
        {
            Ids = [ createdInput!.Id!.Value, 9999 ],
            StatusId = Domain.Constants.Input.InputStatus.Finish
        };

        var response = await _client.PatchAsJsonAsync(
            "/api/v1/InventoryReceipts/status",
            statusRequest,
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.MultiStatus, HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_034 - Xóa phiếu nhập thành công ở trạng thái working")]
    public async Task DeleteInput_WorkingStatus_DeletesSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var createRequest = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest)
            .ConfigureAwait(true);
        var createdInput = await createResponse.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        var response = await _client.DeleteAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}").ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = db.InputReceipts.FirstOrDefault(i => i.Id == createdInput.Id);
        input!.DeletedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "INPUT_038 - Xóa nhiều phiếu nhập cùng lúc")]
    public async Task DeleteManyInputs_MultipleInputs_DeletesAllSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var ids = new List<int>();
        for(int i = 0; i < 3; i++)
        {
            var createRequest = new CreateInputCommand
            {
                Notes = $"Test {i}",
                SupplierId = 1,
                Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
            };
            var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest)
                .ConfigureAwait(true);
            var createdInput = await createResponse.Content
                .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
                .ConfigureAwait(true);
            ids.Add(createdInput!.Id!.Value);
        }

        var deleteRequest = new DeleteManyInputsCommand { Ids = ids };

        var response = await _client.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("/api/v1/InventoryReceipts", UriKind.Relative),
                Content = JsonContent.Create(deleteRequest)
            })
            .ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "INPUT_039 - Khôi phục phiếu nhập đã xóa thành công")]
    public async Task RestoreInput_DeletedInput_RestoresSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var createRequest = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest)
            .ConfigureAwait(true);
        var createdInput = await createResponse.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        await _client.DeleteAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}").ConfigureAwait(true);

        var response = await _client.PostAsync($"/api/v1/InventoryReceipts/{createdInput.Id}/restore", null)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = db.InputReceipts.FirstOrDefault(i => i.Id == createdInput.Id);
        input!.DeletedAt.Should().BeNull();
    }

    [Fact(DisplayName = "INPUT_041 - Khôi phục nhiều phiếu nhập đã xóa cùng lúc")]
    public async Task RestoreManyInputs_MultipleDeletedInputs_RestoresAllSuccessfully()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var ids = new List<int>();
        for(int i = 0; i < 3; i++)
        {
            var createRequest = new CreateInputCommand
            {
                Notes = $"Test {i}",
                SupplierId = 1,
                Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
            };
            var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest)
                .ConfigureAwait(true);
            var createdInput = await createResponse.Content
                .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
                .ConfigureAwait(true);
            ids.Add(createdInput!.Id!.Value);

            await _client.DeleteAsync($"/api/v1/InventoryReceipts/{createdInput.Id}").ConfigureAwait(true);
        }

        var restoreRequest = new RestoreManyInputsCommand { Ids = ids };

        var response = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts/restore", restoreRequest)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        foreach(var id in ids)
        {
            var input = db.InputReceipts.FirstOrDefault(i => i.Id == id);
            input!.DeletedAt.Should().BeNull();
        }
    }

    [Fact(DisplayName = "INPUT_042 - Clone phiếu nhập thành công")]
    public async Task CloneInput_ValidInput_CreatesNewInput()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var createRequest = new CreateInputCommand
        {
            Notes = "Original",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest)
            .ConfigureAwait(true);
        var createdInput = await createResponse.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        var response = await _client.PostAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}/clone", null)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var clonedInput = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        clonedInput!.Id.Should().NotBe(createdInput.Id);
        clonedInput.StatusId.Should().Be(Domain.Constants.Input.InputStatus.Working);
        clonedInput.SupplierId.Should().Be(createdInput.SupplierId);
    }

    [Fact(DisplayName = "INPUT_043 - Clone phiếu nhập với sản phẩm đã bị xóa")]
    public async Task CloneInput_WithDeletedProduct_ClonesOnlyValidProducts()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var createRequest = new CreateInputCommand
        {
            Notes = "Original",
            SupplierId = 1,
            Products =
                [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }, new CreateInputInfoRequest
                {
                    ProductId = 2,
                    Count = 5,
                    InputPrice = 50000
                } ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest)
            .ConfigureAwait(true);
        var createdInput = await createResponse.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var product = db.Products.FirstOrDefault(p => p.Id == 2);
        if(product != null)
        {
            product.DeletedAt = DateTimeOffset.UtcNow.DateTime;
            db.SaveChanges();
        }

        var response = await _client.PostAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}/clone", null)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var clonedInput = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        clonedInput!.Products.Should().HaveCount(1);
        clonedInput.Products[0].ProductId.Should().Be(1);
    }

    [Fact(DisplayName = "INPUT_046 - Lấy danh sách phiếu nhập theo SupplierId")]
    public async Task GetInputsBySupplierId_ValidSupplierId_ReturnsFilteredInputs()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        int supplierId = 1;

        var response = await _client.GetAsync($"/api/v1/InventoryReceipts/by-supplier/{supplierId}?page=1&pageSize=10")
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<InputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().OnlyContain(i => i.SupplierId == supplierId);
    }

    [Fact(DisplayName = "INPUT_047 - Lấy danh sách phiếu nhập đã xóa")]
    public async Task GetDeletedInputs_ReturnsOnlyDeletedInputs()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var response = await _client.GetAsync("/api/v1/InventoryReceipts/deleted?page=1&pageSize=10")
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<InputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
    }

    [Fact(DisplayName = "INPUT_066 - Tạo phiếu nhập với ImportPrice là 0")]
    public async Task CreateInput_ZeroImportPrice_AllowsFreeProducts()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var request = new CreateInputCommand
        {
            Notes = "Free products",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 0 } ]
        };

        var response = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.TotalPayable.Should().Be(0);
    }

    [Fact(DisplayName = "INPUT_068 - Cập nhật trạng thái phiếu nhập với ConfirmedBy được ghi nhận đúng")]
    public async Task UpdateInputStatus_TracksConfirmedByCorrectly()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var createRequest = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest)
            .ConfigureAwait(true);
        var createdInput = await createResponse.Content
            .ReadFromJsonAsync<InputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        var statusRequest = new UpdateInputStatusCommand { StatusId = Domain.Constants.Input.InputStatus.Finish };

        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/InventoryReceipts/{createdInput!.Id}/status",
            statusRequest,
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = db.InputReceipts.FirstOrDefault(i => i.Id == createdInput.Id);
        input!.ConfirmedBy.Should().NotBeNull();
    }

    [Fact(DisplayName = "INPUT_069 - Lấy danh sách phiếu nhập với nhiều filter kết hợp")]
    public async Task GetInputs_MultipleCombinedFilters_ReturnsFilteredResults()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Brands.Create], email);
        var loginResponse = await IntegrationTestHelper.AuthenticateAsync(_client, username, password);

        var response = await _client.GetAsync("/api/v1/InventoryReceipts?filters=StatusId==working,SupplierId==1")
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content
            .ReadFromJsonAsync<PagedResult<InputResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items
            .Should()
            .OnlyContain(
                i => string.Compare(i.StatusId, Domain.Constants.Input.InputStatus.Working) == 0 && i.SupplierId == 1);
    }
#pragma warning restore CRR0035
}
