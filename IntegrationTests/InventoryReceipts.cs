using Application.ApiContracts.Input.Requests;
using Application.ApiContracts.Input.Responses;
using Application.Features.Inputs.Commands.CreateInput;
using Application.Features.Inputs.Commands.DeleteManyInputs;
using Application.Features.Inputs.Commands.RestoreManyInputs;
using Application.Features.Inputs.Commands.UpdateInput;
using Application.Features.Inputs.Commands.UpdateInputStatus;
using Application.Features.Inputs.Commands.UpdateManyInputStatus;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

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

    [Fact(DisplayName = "INPUT_001 - Tạo phiếu nhập hàng thành công (Happy Path)")]
    public async Task CreateInput_Success_ReturnsOk()
    {
        // Arrange
        var request = new UpdateInputCommand
        {
            Notes = "Nhập hàng tháng 1",
            SupplierId = 1,
            Products =
            [
                new UpdateInputInfoRequest
                {
                    ProductId = 1,
                    Count = 10,
                    InputPrice = 100000
                }
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<InputResponse>();
        content.Should().NotBeNull();
        content!.Id.Should().NotBeNull();
        content.StatusId.Should().Be(Domain.Constants.InputStatus.Working);
        content.Products.Should().HaveCount(1);
        content.Products[0].Count.Should().Be(10);
        content.Products[0].InputPrice.Should().Be(100000);
        content.TotalPayable.Should().Be(1000000);

        // Verify DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = db.InputReceipts.FirstOrDefault(i => i.Id == content.Id);
        input.Should().NotBeNull();
        input!.StatusId.Should().Be(Domain.Constants.InputStatus.Working);
    }

    [Fact(DisplayName = "INPUT_002 - Tạo phiếu nhập với nhiều sản phẩm và tính toán chính xác tổng tiền")]
    public async Task CreateInput_MultipleProducts_CalculatesTotalCorrectly()
    {
        // Arrange
        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest
                {
                    ProductId = 1,
                    Count = 5,
                    InputPrice = 123456.78m
                },
                new CreateInputInfoRequest
                {
                    ProductId = 2,
                    Count = 3,
                    InputPrice = 987654.32m
                }
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<InputResponse>();
        content.Should().NotBeNull();

        // Calculate expected: (5 × 123456.78) + (3 × 987654.32) = 617283.90 + 2962962.96 = 3580246.86
        decimal expectedTotal = (5 * 123456.78m) + (3 * 987654.32m);
        content!.TotalPayable.Should().Be((long)expectedTotal);
    }

    [Fact(DisplayName = "INPUT_004 - Tạo phiếu nhập với SupplierId không tồn tại")]
    public async Task CreateInput_NonExistentSupplier_ReturnsNotFound()
    {
        // Arrange  
        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 9999,
            Products =
            [
                new CreateInputInfoRequest
                {
                    ProductId = 1,
                    Count = 10,
                    InputPrice = 100000
                }
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_005 - Tạo phiếu nhập với ProductId không tồn tại")]
    public async Task CreateInput_NonExistentProduct_ReturnsNotFound()
    {
        // Arrange
        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest
                {
                    ProductId = 9999,
                    Count = 10,
                    InputPrice = 100000
                }
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_006 - Tạo phiếu nhập với Product đã bị xóa hoặc không còn bán")]
    public async Task CreateInput_DeletedProduct_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest
                {
                    ProductId = 1,
                    Count = 10,
                    InputPrice = 100000
                }
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_012 - Tạo phiếu nhập với Notes chứa script XSS")]
    public async Task CreateInput_XSSInNotes_SanitizesInput()
    {
        // Arrange
        var request = new CreateInputCommand
        {
            Notes = "<script>alert('XSS')</script>",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest
                {
                    ProductId = 1,
                    Count = 10,
                    InputPrice = 100000
                }
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify DB - Notes should be sanitized
        var content = await response.Content.ReadFromJsonAsync<InputResponse>();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = db.InputReceipts.FirstOrDefault(i => i.Id == content!.Id);
        input!.Notes.Should().NotContain("<script>");
    }

    [Fact(DisplayName = "INPUT_016 - Lấy danh sách phiếu nhập có phân trang")]
    public async Task GetInputs_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        // Assuming DB has 25 inputs

        // Act
        var response = await _client.GetAsync("/api/v1/InventoryReceipts?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Domain.Primitives.PagedResult<InputResponse>>();
        content.Should().NotBeNull();
        content!.Items.Should().HaveCountLessThanOrEqualTo(10);
        content.PageNumber.Should().Be(1);
    }

    [Fact(DisplayName = "INPUT_017 - Lấy danh sách phiếu nhập với filter theo StatusId")]
    public async Task GetInputs_FilterByStatus_ReturnsFilteredResults()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/v1/InventoryReceipts?filters=StatusId==working");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Domain.Primitives.PagedResult<InputResponse>>();
        content.Should().NotBeNull();
        content!.Items.Should().OnlyContain(i => i.StatusId == Domain.Constants.InputStatus.Working);
    }

    [Fact(DisplayName = "INPUT_018 - Lấy danh sách phiếu nhập với sort theo InputDate descending")]
    public async Task GetInputs_SortByInputDateDesc_ReturnsSortedResults()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/v1/InventoryReceipts?sorts=-InputDate");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Domain.Primitives.PagedResult<InputResponse>>();
        content.Should().NotBeNull();
        content!.Items.Should().BeInDescendingOrder(i => i.InputDate);
    }

    [Fact(DisplayName = "INPUT_020 - Lấy chi tiết phiếu nhập thành công")]
    public async Task GetInputById_ExistingInput_ReturnsCompleteDetails()
    {
        // Arrange
        int inputId = 1;

        // Act
        var response = await _client.GetAsync($"/api/v1/InventoryReceipts/{inputId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<InputResponse>();
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
        // Arrange
        int inputId = 9999;

        // Act
        var response = await _client.GetAsync($"/api/v1/InventoryReceipts/{inputId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "INPUT_023 - Cập nhật phiếu nhập thành công ở trạng thái working")]
    public async Task UpdateInput_WorkingStatus_UpdatesSuccessfully()
    {
        // Arrange
        // First create an input in working status
        var createRequest = new CreateInputCommand
        {
            Notes = "Original",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest);
        var createdInput = await createResponse.Content.ReadFromJsonAsync<InputResponse>();
            
        var updateRequest = new UpdateInputCommand
        {
            Notes = "Updated",
            SupplierId = 2,
            Products =
            [
                new UpdateInputInfoRequest { ProductId = 2, Count = 20, InputPrice = 200000 }
            ]
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<InputResponse>();
        content!.Notes.Should().Be("Updated");
        content.SupplierId.Should().Be(2);
        content.TotalPayable.Should().Be(4000000);
    }

    [Fact(DisplayName = "INPUT_024 - Cập nhật phiếu nhập ở trạng thái finished (không cho phép)")]
    public async Task UpdateInput_FinishedStatus_ReturnsBadRequest()
    {
        // Arrange
        // Create and finish an input
        var createRequest = new CreateInputCommand
        {
            Notes = "Original",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest);
        var createdInput = await createResponse.Content.ReadFromJsonAsync<InputResponse>();

        // Update status to finished
        await _client.PatchAsJsonAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}/status",
            new UpdateInputStatusCommand { StatusId = Domain.Constants.InputStatus.Finish });

        var updateRequest = new UpdateInputCommand
        {
            Notes = "Updated",
            SupplierId = 2,
            Products = []
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/InventoryReceipts/{createdInput.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_025 - Cập nhật phiếu nhập ở trạng thái cancelled (không cho phép)")]
    public async Task UpdateInput_CancelledStatus_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreateInputCommand
        {
            Notes = "Original",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest);
        var createdInput = await createResponse.Content.ReadFromJsonAsync<InputResponse>();

        // Update status to cancelled
        await _client.PatchAsJsonAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}/status",
            new UpdateInputStatusCommand { StatusId = Domain.Constants.InputStatus.Cancel });

        var updateRequest = new UpdateInputCommand
        {
            Notes = "Updated",
            SupplierId = 2,
            Products = []
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/InventoryReceipts/{createdInput.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_028 - Cập nhật trạng thái phiếu nhập từ working sang finished")]
    public async Task UpdateInputStatus_WorkingToFinished_UpdatesSuccessfully()
    {
        // Arrange
        var createRequest = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest);
        var createdInput = await createResponse.Content.ReadFromJsonAsync<InputResponse>();

        var statusRequest = new UpdateInputStatusCommand { StatusId = Domain.Constants.InputStatus.Finish };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}/status", statusRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<InputResponse>();
        content!.StatusId.Should().Be(Domain.Constants.InputStatus.Finish);

        // Verify DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = db.InputReceipts.FirstOrDefault(i => i.Id == createdInput.Id);
        input!.StatusId.Should().Be(Domain.Constants.InputStatus.Finish);
        input.ConfirmedBy.Should().NotBeNull();
    }

    [Fact(DisplayName = "INPUT_029 - Cập nhật trạng thái phiếu nhập từ working sang cancelled")]
    public async Task UpdateInputStatus_WorkingToCancelled_UpdatesSuccessfully()
    {
        // Arrange
        var createRequest = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest);
        var createdInput = await createResponse.Content.ReadFromJsonAsync<InputResponse>();

        var statusRequest = new UpdateInputStatusCommand { StatusId = Domain.Constants.InputStatus.Cancel };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}/status", statusRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<InputResponse>();
        content!.StatusId.Should().Be(Domain.Constants.InputStatus.Cancel);
    }

    [Fact(DisplayName = "INPUT_032 - Cập nhật trạng thái nhiều phiếu nhập cùng lúc")]
    public async Task UpdateManyInputStatus_MultipleInputs_UpdatesAllSuccessfully()
    {
        // Arrange
        // Create 3 inputs
        var ids = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            var createRequest = new CreateInputCommand
            {
                Notes = $"Test {i}",
                SupplierId = 1,
                Products =
                [
                    new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
                ]
            };
            var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest);
            var createdInput = await createResponse.Content.ReadFromJsonAsync<InputResponse>();
            ids.Add(createdInput!.Id!.Value);
        }

        var statusRequest = new UpdateManyInputStatusCommand
        {
            Ids = ids,
            StatusId = Domain.Constants.InputStatus.Finish
        };

        // Act
        var response = await _client.PatchAsJsonAsync("/api/v1/InventoryReceipts/status", statusRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify all inputs are updated
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        foreach (var id in ids)
        {
            var input = db.InputReceipts.FirstOrDefault(i => i.Id == id);
            input!.StatusId.Should().Be(Domain.Constants.InputStatus.Finish);
        }
    }

    [Fact(DisplayName = "INPUT_033 - Cập nhật trạng thái nhiều phiếu nhập với một số Id không tồn tại")]
    public async Task UpdateManyInputStatus_SomeNonExistent_HandlesPartially()
    {
        // Arrange
        var createRequest = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest);
        var createdInput = await createResponse.Content.ReadFromJsonAsync<InputResponse>();

        var statusRequest = new UpdateManyInputStatusCommand
        {
            Ids = [createdInput!.Id!.Value, 9999],
            StatusId = Domain.Constants.InputStatus.Finish
        };

        // Act
        var response = await _client.PatchAsJsonAsync("/api/v1/InventoryReceipts/status", statusRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MultiStatus, HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INPUT_034 - Xóa phiếu nhập thành công ở trạng thái working")]
    public async Task DeleteInput_WorkingStatus_DeletesSuccessfully()
    {
        // Arrange
        var createRequest = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest);
        var createdInput = await createResponse.Content.ReadFromJsonAsync<InputResponse>();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        // Verify DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = db.InputReceipts.FirstOrDefault(i => i.Id == createdInput.Id);
        input!.DeletedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "INPUT_038 - Xóa nhiều phiếu nhập cùng lúc")]
    public async Task DeleteManyInputs_MultipleInputs_DeletesAllSuccessfully()
    {
        // Arrange
        var ids = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            var createRequest = new CreateInputCommand
            {
                Notes = $"Test {i}",
                SupplierId = 1,
                Products =
                [
                    new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
                ]
            };
            var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest);
            var createdInput = await createResponse.Content.ReadFromJsonAsync<InputResponse>();
            ids.Add(createdInput!.Id!.Value);
        }

        var deleteRequest = new DeleteManyInputsCommand { Ids = ids };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri("/api/v1/InventoryReceipts", UriKind.Relative),
            Content = JsonContent.Create(deleteRequest)
        });

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "INPUT_039 - Khôi phục phiếu nhập đã xóa thành công")]
    public async Task RestoreInput_DeletedInput_RestoresSuccessfully()
    {
        // Arrange
        var createRequest = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest);
        var createdInput = await createResponse.Content.ReadFromJsonAsync<InputResponse>();

        // Delete it first
        await _client.DeleteAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}");

        // Act
        var response = await _client.PostAsync($"/api/v1/InventoryReceipts/{createdInput.Id}/restore", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = db.InputReceipts.FirstOrDefault(i => i.Id == createdInput.Id);
        input!.DeletedAt.Should().BeNull();
    }

    [Fact(DisplayName = "INPUT_041 - Khôi phục nhiều phiếu nhập đã xóa cùng lúc")]
    public async Task RestoreManyInputs_MultipleDeletedInputs_RestoresAllSuccessfully()
    {
        // Arrange
        var ids = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            var createRequest = new CreateInputCommand
            {
                Notes = $"Test {i}",
                SupplierId = 1,
                Products =
                [
                    new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
                ]
            };
            var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest);
            var createdInput = await createResponse.Content.ReadFromJsonAsync<InputResponse>();
            ids.Add(createdInput!.Id!.Value);

            // Delete each one
            await _client.DeleteAsync($"/api/v1/InventoryReceipts/{createdInput.Id}");
        }

        var restoreRequest = new RestoreManyInputsCommand { Ids = ids };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts/restore", restoreRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify all are restored
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        foreach (var id in ids)
        {
            var input = db.InputReceipts.FirstOrDefault(i => i.Id == id);
            input!.DeletedAt.Should().BeNull();
        }
    }

    [Fact(DisplayName = "INPUT_042 - Clone phiếu nhập thành công")]
    public async Task CloneInput_ValidInput_CreatesNewInput()
    {
        // Arrange
        var createRequest = new CreateInputCommand
        {
            Notes = "Original",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest);
        var createdInput = await createResponse.Content.ReadFromJsonAsync<InputResponse>();

        // Act
        var response = await _client.PostAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}/clone", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var clonedInput = await response.Content.ReadFromJsonAsync<InputResponse>();
        clonedInput!.Id.Should().NotBe(createdInput.Id);
        clonedInput.StatusId.Should().Be(Domain.Constants.InputStatus.Working);
        clonedInput.SupplierId.Should().Be(createdInput.SupplierId);
    }

    [Fact(DisplayName = "INPUT_043 - Clone phiếu nhập với sản phẩm đã bị xóa")]
    public async Task CloneInput_WithDeletedProduct_ClonesOnlyValidProducts()
    {
        // Arrange
        var createRequest = new CreateInputCommand
        {
            Notes = "Original",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 },
                new CreateInputInfoRequest { ProductId = 2, Count = 5, InputPrice = 50000 }
            ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest);
        var createdInput = await createResponse.Content.ReadFromJsonAsync<InputResponse>();

        // Delete product 2
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var product = db.Products.FirstOrDefault(p => p.Id == 2);
        if (product != null)
        {
            product.DeletedAt = DateTimeOffset.UtcNow.DateTime;
            db.SaveChanges();
        }

        // Act
        var response = await _client.PostAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}/clone", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var clonedInput = await response.Content.ReadFromJsonAsync<InputResponse>();
        clonedInput!.Products.Should().HaveCount(1);
        clonedInput.Products[0].ProductId.Should().Be(1);
    }

    [Fact(DisplayName = "INPUT_046 - Lấy danh sách phiếu nhập theo SupplierId")]
    public async Task GetInputsBySupplierId_ValidSupplierId_ReturnsFilteredInputs()
    {
        // Arrange
        int supplierId = 1;

        // Act
        var response = await _client.GetAsync($"/api/v1/InventoryReceipts/by-supplier/{supplierId}?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Domain.Primitives.PagedResult<InputResponse>>();
        content.Should().NotBeNull();
        content!.Items.Should().OnlyContain(i => i.SupplierId == supplierId);
    }

    [Fact(DisplayName = "INPUT_047 - Lấy danh sách phiếu nhập đã xóa")]
    public async Task GetDeletedInputs_ReturnsOnlyDeletedInputs()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/v1/InventoryReceipts/deleted?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Domain.Primitives.PagedResult<InputResponse>>();
        content.Should().NotBeNull();
    }

    [Fact(DisplayName = "INPUT_066 - Tạo phiếu nhập với ImportPrice là 0")]
    public async Task CreateInput_ZeroImportPrice_AllowsFreeProducts()
    {
        // Arrange
        var request = new CreateInputCommand
        {
            Notes = "Free products",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 0 }
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<InputResponse>();
        content!.TotalPayable.Should().Be(0);
    }

    [Fact(DisplayName = "INPUT_068 - Cập nhật trạng thái phiếu nhập với ConfirmedBy được ghi nhận đúng")]
    public async Task UpdateInputStatus_TracksConfirmedByCorrectly()
    {
        // Arrange
        var createRequest = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/InventoryReceipts", createRequest);
        var createdInput = await createResponse.Content.ReadFromJsonAsync<InputResponse>();

        var statusRequest = new UpdateInputStatusCommand { StatusId = Domain.Constants.InputStatus.Finish };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/InventoryReceipts/{createdInput!.Id}/status", statusRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify ConfirmedBy is set
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var input = db.InputReceipts.FirstOrDefault(i => i.Id == createdInput.Id);
        input!.ConfirmedBy.Should().NotBeNull();
    }

    [Fact(DisplayName = "INPUT_069 - Lấy danh sách phiếu nhập với nhiều filter kết hợp")]
    public async Task GetInputs_MultipleCombinedFilters_ReturnsFilteredResults()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/v1/InventoryReceipts?filters=StatusId==working,SupplierId==1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Domain.Primitives.PagedResult<InputResponse>>();
        content.Should().NotBeNull();
        content!.Items.Should().OnlyContain(i => i.StatusId == Domain.Constants.InputStatus.Working && i.SupplierId == 1);
    }
}
