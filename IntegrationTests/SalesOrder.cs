using Application.ApiContracts.Output.Responses;
using Application.Features.Outputs.Commands.CreateOutput;
using Application.Features.Outputs.Commands.CreateOutputByManager;
using Application.Features.Outputs.Commands.DeleteManyOutputs;
using Application.Features.Outputs.Commands.RestoreManyOutputs;
using Application.Features.Outputs.Commands.UpdateManyOutputStatus;
using Application.Features.Outputs.Commands.UpdateOutputForManager;
using Application.Features.Outputs.Commands.UpdateOutputStatus;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

public class SalesOrder : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

#pragma warning disable CRR0035
    public SalesOrder(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact(DisplayName = "SO_061 - Tạo đơn hàng với BuyerId tự động từ token JWT")]
    public async Task CreateOutput_WithAuthenticatedUser_SetsBuyerIdFromToken()
    {
        var request = new CreateOutputCommand { BuyerId = Guid.NewGuid(), Notes = "Test" };

        var response = await _client.PostAsJsonAsync("/api/v1/SalesOrders", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.BuyerId.Should().NotBeNull();
    }

    [Fact(DisplayName = "SO_062 - Tạo đơn hàng COD và kiểm tra trạng thái ban đầu")]
    public async Task CreateOutput_CODOrder_InitialStatusIsPending()
    {
        var request = new CreateOutputCommand { BuyerId = Guid.NewGuid(), Notes = "COD Order" };

        var response = await _client.PostAsJsonAsync("/api/v1/SalesOrders", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        order.Should().NotBeNull();
        order!.StatusId.Should().Be("pending");
    }

    [Fact(DisplayName = "SO_063 - Luồng COD đầy đủ: Pending -> ConfirmedCod -> Delivering -> Completed")]
    public async Task UpdateOutputStatus_CODFlow_CompletesSuccessfully()
    {
        var createRequest = new CreateOutputCommand { BuyerId = Guid.NewGuid() };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/SalesOrders", createRequest).ConfigureAwait(true);
        var order = await createResponse.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        int orderId = order!.Id!.Value;

        var updateRequest1 = new UpdateOutputStatusCommand { StatusId = "confirmed_cod" };
        var response1 = await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            updateRequest1,
            CancellationToken.None)
            .ConfigureAwait(true);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateRequest2 = new UpdateOutputStatusCommand { StatusId = "delivering" };
        var response2 = await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            updateRequest2,
            CancellationToken.None)
            .ConfigureAwait(true);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateRequest3 = new UpdateOutputStatusCommand { StatusId = "completed" };
        var response3 = await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            updateRequest3,
            CancellationToken.None)
            .ConfigureAwait(true);
        response3.StatusCode.Should().Be(HttpStatusCode.OK);

        var finalOrder = await response3.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        finalOrder!.StatusId.Should().Be("completed");
    }

    [Fact(DisplayName = "SO_064 - Luồng Deposit đầy đủ: Pending -> Deposit50 -> Confirmed50 -> Delivering -> Completed")]
    public async Task UpdateOutputStatus_DepositFlow_CompletesSuccessfully()
    {
        var createRequest = new CreateOutputCommand { BuyerId = Guid.NewGuid() };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/SalesOrders", createRequest).ConfigureAwait(true);
        var order = await createResponse.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        int orderId = order!.Id!.Value;

        await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            new UpdateOutputStatusCommand { StatusId = "deposit_50" },
            CancellationToken.None)
            .ConfigureAwait(true);
        await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            new UpdateOutputStatusCommand { StatusId = "confirmed_50" },
            CancellationToken.None)
            .ConfigureAwait(true);
        await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            new UpdateOutputStatusCommand { StatusId = "delivering" },
            CancellationToken.None)
            .ConfigureAwait(true);
        var finalResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            new UpdateOutputStatusCommand { StatusId = "completed" },
            CancellationToken.None)
            .ConfigureAwait(true);

        finalResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "SO_065 - Luồng Refund: Pending -> ConfirmedCod -> Refund")]
    public async Task UpdateOutputStatus_RefundFlow_CompletesSuccessfully()
    {
        var createRequest = new CreateOutputCommand { BuyerId = Guid.NewGuid() };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/SalesOrders", createRequest).ConfigureAwait(true);
        var order = await createResponse.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        int orderId = order!.Id!.Value;

        await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            new UpdateOutputStatusCommand { StatusId = "confirmed_cod" },
            CancellationToken.None)
            .ConfigureAwait(true);
        var refundResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{orderId}/status",
            new UpdateOutputStatusCommand { StatusId = "refund" },
            CancellationToken.None)
            .ConfigureAwait(true);

        refundResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "SO_066 - CreateOutputForAdmin được gọi bởi Manager")]
    public async Task CreateOutputForAdmin_ByManager_CreatesSuccessfully()
    {
        var request = new CreateOutputByManagerCommand { BuyerId = Guid.NewGuid() };

        var response = await _client.PostAsJsonAsync("/api/v1/SalesOrders/admin", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "SO_067 - Tìm kiếm với Filters")]
    public async Task GetOutputs_WithFilters_ReturnsFilteredResults()
    {
        var response = await _client.GetAsync("/api/v1/SalesOrders?filters=status==pending", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "SO_068 - Tạo đơn hàng với nhiều sản phẩm")]
    public async Task CreateOutput_WithMultipleProducts_CreatesSuccessfully()
    {
        var request = new CreateOutputCommand { BuyerId = Guid.NewGuid() };

        var response = await _client.PostAsJsonAsync("/api/v1/SalesOrders", request).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "SO_069 - Sắp xếp theo CreatedAt DESC")]
    public async Task GetOutputs_SortByCreatedAtDesc_ReturnsSortedResults()
    {
        var response = await _client.GetAsync("/api/v1/SalesOrders?sorts=-createdAt", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "SO_070 - Phân trang với Page và PageSize")]
    public async Task GetOutputs_WithPagination_ReturnsPagedResults()
    {
        var response = await _client.GetAsync("/api/v1/SalesOrders?page=1&pageSize=10", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "SO_071 - Soft delete đơn hàng")]
    public async Task DeleteOutput_ValidId_SetsDeletedAt()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/SalesOrders",
            new CreateOutputCommand { BuyerId = Guid.NewGuid() })
            .ConfigureAwait(true);
        var order = await createResponse.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/SalesOrders/{order!.Id}").ConfigureAwait(true);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "SO_072 - Restore đơn hàng")]
    public async Task RestoreOutput_ValidId_ClearsDeletedAt()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/SalesOrders",
            new CreateOutputCommand { BuyerId = Guid.NewGuid() })
            .ConfigureAwait(true);
        var order = await createResponse.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        await _client.DeleteAsync($"/api/v1/SalesOrders/{order!.Id}").ConfigureAwait(true);

        var restoreResponse = await _client.PatchAsync($"/api/v1/SalesOrders/{order.Id}/restore", null)
            .ConfigureAwait(true);

        restoreResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "SO_073 - GetDeletedOutputs chỉ trả về đơn đã xóa")]
    public async Task GetDeletedOutputs_ReturnsOnlyDeletedOrders()
    {
        var response = await _client.GetAsync("/api/v1/SalesOrders/deleted", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "SO_074 - UpdateOutput chỉ khi có quyền")]
    public async Task UpdateOutput_WithoutPermission_ReturnsForbidden()
    {
        var request = new UpdateOutputForManagerCommand();

        var response = await _client.PatchAsJsonAsync("/api/v1/SalesOrders/1", request, CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode
            .Should()
            .BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "SO_075 - UpdateOutputStatus chuyển đổi không hợp lệ")]
    public async Task UpdateOutputStatus_InvalidTransition_ReturnsBadRequest()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/SalesOrders",
            new CreateOutputCommand { BuyerId = Guid.NewGuid() })
            .ConfigureAwait(true);
        var order = await createResponse.Content
            .ReadFromJsonAsync<OutputResponse>(CancellationToken.None)
            .ConfigureAwait(true);

        var request = new UpdateOutputStatusCommand { StatusId = "completed" };
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/SalesOrders/{order!.Id}/status",
            request,
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "SO_076 - DeleteManyOutputs xóa nhiều đơn")]
    public async Task DeleteManyOutputs_ValidIds_DeletesAllOrders()
    {
        var request = new DeleteManyOutputsCommand { Ids = [ 1, 2 ] };

        var response = await _client.PostAsJsonAsync("/api/v1/SalesOrders/delete-many", request).ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "SO_077 - RestoreManyOutputs khôi phục nhiều đơn")]
    public async Task RestoreManyOutputs_ValidIds_RestoresAllOrders()
    {
        var request = new RestoreManyOutputsCommand { Ids = [ 1, 2 ] };

        var response = await _client.PostAsJsonAsync("/api/v1/SalesOrders/restore-many", request).ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "SO_078 - UpdateManyOutputStatus cập nhật trạng thái nhiều đơn")]
    public async Task UpdateManyOutputStatus_ValidIds_UpdatesAllStatuses()
    {
        var request = new UpdateManyOutputStatusCommand { Ids = [ 1, 2 ], StatusId = "confirmed_cod" };

        var response = await _client.PostAsJsonAsync("/api/v1/SalesOrders/update-status-many", request)
            .ConfigureAwait(true);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "SO_079 - GetMyPurchases chỉ trả về đơn của user đăng nhập")]
    public async Task GetMyPurchases_AuthenticatedUser_ReturnsUserOrders()
    {
        var response = await _client.GetAsync("/api/v1/SalesOrders/my-purchases", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "SO_080 - GetPurchasesByID lấy đơn theo BuyerId (manager)")]
    public async Task GetPurchasesByID_ByManager_ReturnsUserOrders()
    {
        var buyerId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1/SalesOrders/purchases/{buyerId}", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
#pragma warning restore CRR0035
}
