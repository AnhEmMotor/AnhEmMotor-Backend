# Hướng Dẫn Hoàn Thiện Dự Án AnhEmMotor-Backend

## Tổng Quan

Dự án này yêu cầu tạo thêm khoảng **150+ files** với **15,000+ dòng code**. Các file cơ bản đã được tạo, bạn cần hoàn thiện phần còn lại theo pattern đã có.

## ✅ Đã Hoàn Thành

### 1. Domain Layer

- ✅ Entities: Input, InputInfo, InputStatus, Output, OutputInfo, OutputStatus (đã có sẵn)
- ✅ Constants: InputStatus, OrderStatus, OrderStatusTransitions
- ✅ ProductStatus, SupplierStatus constants

### 2. Application Layer - API Contracts

- ✅ Input: InputResponse, CreateInputRequest, UpdateInputRequest, UpdateInputStatusRequest, UpdateManyInputStatusRequest, DeleteManyInputsRequest, RestoreManyInputsRequest, InputInfoDto
- ✅ Output: OutputResponse, CreateOutputRequest, UpdateOutputRequest, UpdateOutputStatusRequest, UpdateManyOutputStatusRequest, DeleteManyOutputsRequest, RestoreManyOutputsRequest, OutputInfoDto
- ✅ Supplier: SupplierResponse (đã thêm TotalInput field)

### 3. Application Layer - Repository Interfaces

- ✅ IInputReadRepository, IInputInsertRepository, IInputUpdateRepository, IInputDeleteRepository
- ✅ IOutputReadRepository, IOutputInsertRepository, IOutputUpdateRepository, IOutputDeleteRepository
- ✅ IStatisticalReadRepository (với tất cả DTOs)

### 4. Infrastructure Layer - Repository Implementations

- ✅ Input: InputReadRepository, InputInsertRepository, InputUpdateRepository, InputDeleteRepository
- ✅ Output: OutputReadRepository, OutputInsertRepository, OutputUpdateRepository (bao gồm COGS FIFO logic), OutputDeleteRepository
- ✅ Statistical: StatisticalReadRepository (với đầy đủ 6 methods)

### 5. Infrastructure - Dependency Injection

- ✅ Đã register tất cả repositories trong DBContext.cs

### 6. Application Layer - Mappings

- ✅ InputMappingConfig (partial)
- ✅ OutputMappingConfig (partial)
- ✅ CustomSieveProcessor (đã thêm mapping cho Input và Output)

### 7. Application Layer - MediatR Handlers (Partial)

- ✅ CreateInputCommand và CreateInputCommandHandler (mẫu)

## ❌ Cần Hoàn Thiện

### 1. Application/Features/Inputs

#### Commands (Cần tạo):

```
Commands/
├── CreateInput/
│   ├── CreateInputCommand.cs ✅
│   ├── CreateInputCommandHandler.cs ✅
│   └── CreateInputCommandValidator.cs (tùy chọn)
│
├── UpdateInput/
│   ├── UpdateInputCommand.cs
│   ├── UpdateInputCommandHandler.cs
│   └── UpdateInputCommandValidator.cs (tùy chọn)
│
├── UpdateInputStatus/
│   ├── UpdateInputStatusCommand.cs
│   ├── UpdateInputStatusCommandHandler.cs
│   └── UpdateInputStatusCommandValidator.cs (tùy chọn)
│
├── UpdateManyInputStatus/
│   ├── UpdateManyInputStatusCommand.cs
│   ├── UpdateManyInputStatusCommandHandler.cs
│   └── UpdateManyInputStatusCommandValidator.cs (tùy chọn)
│
├── DeleteInput/
│   ├── DeleteInputCommand.cs
│   ├── DeleteInputCommandHandler.cs
│
├── DeleteManyInputs/
│   ├── DeleteManyInputsCommand.cs
│   ├── DeleteManyInputsCommandHandler.cs
│
├── RestoreInput/
│   ├── RestoreInputCommand.cs
│   ├── RestoreInputCommandHandler.cs
│
└── RestoreManyInputs/
    ├── RestoreManyInputsCommand.cs
    └── RestoreManyInputsCommandHandler.cs
```

#### Queries (Cần tạo):

```
Queries/
├── GetInputsList/
│   ├── GetInputsListQuery.cs
│   └── GetInputsListQueryHandler.cs
│
├── GetDeletedInputsList/
│   ├── GetDeletedInputsListQuery.cs
│   └── GetDeletedInputsListQueryHandler.cs
│
├── GetInputById/
│   ├── GetInputByIdQuery.cs
│   └── GetInputByIdQueryHandler.cs
│
└── GetInputsBySupplierId/
    ├── GetInputsBySupplierIdQuery.cs
    └── GetInputsBySupplierIdQueryHandler.cs
```

### 2. Application/Features/Outputs

#### Commands (Cần tạo - tương tự Input):

```
Commands/
├── CreateOutput/
│   ├── CreateOutputCommand.cs
│   ├── CreateOutputCommandHandler.cs
│   └── CreateOutputCommandValidator.cs (tùy chọn)
│
├── UpdateOutput/
│   ├── UpdateOutputCommand.cs
│   ├── UpdateOutputCommandHandler.cs (❗ Cần xử lý status transition logic)
│   └── UpdateOutputCommandValidator.cs (tùy chọn)
│
├── UpdateOutputStatus/
│   ├── UpdateOutputStatusCommand.cs
│   ├── UpdateOutputStatusCommandHandler.cs (❗❗ CỰC KỲ QUAN TRỌNG)
│   └── UpdateOutputStatusCommandValidator.cs (tùy chọn)
│
├── UpdateManyOutputStatus/
│   ├── UpdateManyOutputStatusCommand.cs
│   ├── UpdateManyOutputStatusCommandHandler.cs (❗❗ Transaction required)
│   └── UpdateManyOutputStatusCommandValidator.cs (tùy chọn)
│
├── DeleteOutput/
├── DeleteManyOutputs/
├── RestoreOutput/
└── RestoreManyOutputs/
```

#### Queries (Cần tạo):

```
Queries/
├── GetOutputsList/
│   ├── GetOutputsListQuery.cs
│   └── GetOutputsListQueryHandler.cs
│
├── GetDeletedOutputsList/
│   ├── GetDeletedOutputsListQuery.cs
│   └── GetDeletedOutputsListQueryHandler.cs
│
└── GetOutputById/
    ├── GetOutputByIdQuery.cs
    └── GetOutputByIdQueryHandler.cs
```

### 3. Application/Features/Statistical (Cần tạo toàn bộ)

```
Queries/
├── GetDailyRevenue/
│   ├── GetDailyRevenueQuery.cs
│   └── GetDailyRevenueQueryHandler.cs
│
├── GetDashboardStats/
│   ├── GetDashboardStatsQuery.cs
│   └── GetDashboardStatsQueryHandler.cs
│
├── GetMonthlyRevenueProfit/
│   ├── GetMonthlyRevenueProfitQuery.cs
│   └── GetMonthlyRevenueProfitQueryHandler.cs
│
├── GetOrderStatusCounts/
│   ├── GetOrderStatusCountsQuery.cs
│   └── GetOrderStatusCountsQueryHandler.cs
│
├── GetProductReportLastMonth/
│   ├── GetProductReportLastMonthQuery.cs
│   └── GetProductReportLastMonthQueryHandler.cs
│
└── GetProductStockAndPrice/
    ├── GetProductStockAndPriceQuery.cs
    └── GetProductStockAndPriceQueryHandler.cs
```

### 4. WebAPI/Controllers/V1 (Cần tạo 3 controllers)

```
Controllers/V1/
├── InputController.cs (❗ Cần tạo)
├── OutputController.cs (❗ Cần tạo)
└── StatisticalController.cs (❗ Cần tạo)
```

### 5. Cập nhật SupplierReadRepository

Cần thêm logic tính TotalInput khi query Supplier list. Có 2 cách:

**Cách 1: Thêm method trong ISupplierReadRepository**

```csharp
Task<IEnumerable<SupplierWithTotalInputDto>> GetSuppliersWithTotalInputAsync(
    CancellationToken cancellationToken,
    DataFetchMode mode = DataFetchMode.ActiveOnly);
```

**Cách 2: Sử dụng Mapster AfterMapping**
Trong SupplierMappingConfig, add logic để tính TotalInput từ InputReceipts.

Khuyến nghị: **Cách 1** vì hiệu năng tốt hơn (query 1 lần với LEFT JOIN).

## 📝 Pattern và Quy Tắc

### Pattern cho MediatR Handlers

#### 1. Query Handler Pattern

```csharp
using Application.ApiContracts.Input;
using Domain.Shared;
using MediatR;
using Sieve.Models;

namespace Application.Features.Inputs.Queries.GetInputsList;

public sealed record GetInputsListQuery(SieveModel SieveModel) : IRequest<PagedResult<InputResponse>>;
```

```csharp
using Application.ApiContracts.Input;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Shared;
using MediatR;
using InputEntity = Domain.Entities.Input;

namespace Application.Features.Inputs.Queries.GetInputsList;

public sealed class GetInputsListQueryHandler(
    IInputReadRepository repository,
    IPaginator paginator) : IRequestHandler<GetInputsListQuery, PagedResult<InputResponse>>
{
    public Task<PagedResult<InputResponse>> Handle(
        GetInputsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        return paginator.ApplyAsync<InputEntity, InputResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}
```

#### 2. GetById Pattern

```csharp
using Application.ApiContracts.Input;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInputById;

public sealed record GetInputByIdQuery(int Id) : IRequest<(InputResponse? Data, ErrorResponse? Error)>;
```

```csharp
using Application.ApiContracts.Input;
using Application.Interfaces.Repositories.Input;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInputById;

public sealed class GetInputByIdQueryHandler(
    IInputReadRepository repository) : IRequestHandler<GetInputByIdQuery, (InputResponse? Data, ErrorResponse? Error)>
{
    public async Task<(InputResponse? Data, ErrorResponse? Error)> Handle(
        GetInputByIdQuery request,
        CancellationToken cancellationToken)
    {
        var input = await repository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken)
            .ConfigureAwait(false);

        if(input is null)
        {
            return (null, new ErrorResponse
            {
                StatusCode = 404,
                Message = $"Không tìm thấy phiếu nhập có ID {request.Id}."
            });
        }

        return (input.Adapt<InputResponse>(), null);
    }
}
```

#### 3. Delete Command Pattern

```csharp
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteInput;

public sealed record DeleteInputCommand(int Id) : IRequest<Unit>;
```

```csharp
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteInput;

public sealed class DeleteInputCommandHandler(
    IInputReadRepository readRepository,
    IInputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteInputCommand, Unit>
{
    public async Task<Unit> Handle(
        DeleteInputCommand request,
        CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdAsync(
            request.Id,
            cancellationToken)
            .ConfigureAwait(false);

        if(input is null)
        {
            throw new InvalidOperationException($"Không tìm thấy phiếu nhập có ID {request.Id}.");
        }

        deleteRepository.Delete(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
```

#### 4. Delete Many Pattern (với Transaction Safety)

```csharp
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteManyInputs;

public sealed class DeleteManyInputsCommandHandler(
    IInputReadRepository readRepository,
    IInputDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyInputsCommand, Unit>
{
    public async Task<Unit> Handle(
        DeleteManyInputsCommand request,
        CancellationToken cancellationToken)
    {
        var inputs = await readRepository.GetByIdAsync(
            request.Ids,
            cancellationToken)
            .ConfigureAwait(false);

        var inputsList = inputs.ToList();

        if(inputsList.Count != request.Ids.Count)
        {
            var foundIds = inputsList.Select(i => i.Id).ToList();
            var missingIds = request.Ids.Except(foundIds).ToList();
            throw new InvalidOperationException(
                $"Không tìm thấy {missingIds.Count} phiếu nhập: {string.Join(", ", missingIds)}");
        }

        deleteRepository.Delete(inputsList);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
```

#### 5. Update Pattern (với validation logic cho phiếu nhập/xuất)

**UpdateInput Handler:**

```csharp
public async Task<InputResponse> Handle(
    UpdateInputCommand request,
    CancellationToken cancellationToken)
{
    var input = await readRepository.GetByIdWithDetailsAsync(
        request.Id,
        cancellationToken,
        DataFetchMode.ActiveOnly)
        .ConfigureAwait(false);

    if(input is null)
    {
        throw new InvalidOperationException($"Không tìm thấy phiếu nhập có ID {request.Id}.");
    }

    // Validate StatusId
    if(!string.IsNullOrWhiteSpace(request.StatusId) && !InputStatus.IsValid(request.StatusId))
    {
        throw new InvalidOperationException($"Trạng thái '{request.StatusId}' không hợp lệ.");
    }

    // Validate Supplier if changed
    if(request.SupplierId.HasValue && request.SupplierId != input.SupplierId)
    {
        var supplier = await supplierRepository.GetByIdAsync(
            request.SupplierId.Value,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);

        if(supplier is null || supplier.StatusId != SupplierStatus.Active)
        {
            throw new InvalidOperationException("Nhà cung cấp không hợp lệ hoặc không còn hoạt động.");
        }
    }

    // Bước 1: Map thông tin master
    request.Adapt(input);

    // Bước 2: Phân loại InputInfo
    var existingInfoDict = input.InputInfos.ToDictionary(ii => ii.Id);
    var requestInfoDict = request.Products
        .Where(p => p.Id.HasValue && p.Id > 0)
        .ToDictionary(p => p.Id!.Value);

    // Bước 3: Xóa InputInfo không còn trong request
    var toDelete = input.InputInfos
        .Where(ii => !requestInfoDict.ContainsKey(ii.Id))
        .ToList();

    foreach(var info in toDelete)
    {
        deleteRepository.DeleteInputInfo(info);
        input.InputInfos.Remove(info);
    }

    // Bước 4 & 5: Update existing và Add new
    foreach(var productRequest in request.Products)
    {
        if(productRequest.Id.HasValue && productRequest.Id > 0)
        {
            // Update existing
            if(existingInfoDict.TryGetValue(productRequest.Id.Value, out var existingInfo))
            {
                productRequest.Adapt(existingInfo);
                // Recalculate RemainingCount if Count or InputPrice changed
                // (Logic phụ thuộc business rules)
            }
        }
        else
        {
            // Add new
            var newInfo = productRequest.Adapt<InputInfo>();
            newInfo.RemainingCount = newInfo.Count ?? 0;
            input.InputInfos.Add(newInfo);
        }
    }

    // Bước 6: Save
    updateRepository.Update(input);
    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    var updated = await readRepository.GetByIdWithDetailsAsync(
        input.Id,
        cancellationToken)
        .ConfigureAwait(false);

    return updated!.Adapt<InputResponse>();
}
```

#### 6. UpdateOutputStatus Pattern (CỰC KỲ QUAN TRỌNG)

```csharp
public async Task<OutputResponse> Handle(
    UpdateOutputStatusCommand request,
    CancellationToken cancellationToken)
{
    var output = await readRepository.GetByIdWithDetailsAsync(
        request.Id,
        cancellationToken,
        DataFetchMode.ActiveOnly)
        .ConfigureAwait(false);

    if(output is null)
    {
        throw new InvalidOperationException($"Không tìm thấy đơn hàng có ID {request.Id}.");
    }

    // Validate new status
    if(!OrderStatus.IsValid(request.NewStatusId))
    {
        throw new InvalidOperationException($"Trạng thái '{request.NewStatusId}' không hợp lệ.");
    }

    // Check transition is allowed
    if(!OrderStatusTransitions.IsTransitionAllowed(output.StatusId, request.NewStatusId))
    {
        var allowed = OrderStatusTransitions.GetAllowedTransitions(output.StatusId);
        throw new InvalidOperationException(
            $"Không thể chuyển từ '{output.StatusId}' sang '{request.NewStatusId}'. " +
            $"Chỉ được chuyển sang: {string.Join(", ", allowed)}");
    }

    // If transitioning TO 'completed', check stock and process COGS
    if(request.NewStatusId == OrderStatus.Completed)
    {
        foreach(var outputInfo in output.OutputInfos)
        {
            if(outputInfo.ProductId.HasValue && outputInfo.Count.HasValue)
            {
                var stock = await readRepository.GetStockQuantityByVariantIdAsync(
                    outputInfo.ProductId.Value,
                    cancellationToken)
                    .ConfigureAwait(false);

                if(stock < outputInfo.Count.Value)
                {
                    throw new InvalidOperationException(
                        $"Sản phẩm ID {outputInfo.ProductId} không đủ tồn kho. " +
                        $"Hiện có: {stock}, cần: {outputInfo.Count.Value}");
                }
            }
        }

        // Process COGS FIFO
        await updateRepository.ProcessCOGSForCompletedOrderAsync(
            output.Id,
            cancellationToken)
            .ConfigureAwait(false);
    }

    output.StatusId = request.NewStatusId;
    updateRepository.Update(output);
    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    var updated = await readRepository.GetByIdWithDetailsAsync(
        output.Id,
        cancellationToken)
        .ConfigureAwait(false);

    return updated!.Adapt<OutputResponse>();
}
```

### Pattern cho Controllers

#### InputController Example

```csharp
using Application.ApiContracts.Input;
using Application.Features.Inputs.Commands.CreateInput;
using Application.Features.Inputs.Commands.DeleteInput;
using Application.Features.Inputs.Commands.DeleteManyInputs;
using Application.Features.Inputs.Commands.RestoreInput;
using Application.Features.Inputs.Commands.RestoreManyInputs;
using Application.Features.Inputs.Commands.UpdateInput;
using Application.Features.Inputs.Commands.UpdateInputStatus;
using Application.Features.Inputs.Commands.UpdateManyInputStatus;
using Application.Features.Inputs.Queries.GetDeletedInputsList;
using Application.Features.Inputs.Queries.GetInputById;
using Application.Features.Inputs.Queries.GetInputsBySupplierId;
using Application.Features.Inputs.Queries.GetInputsList;
using Asp.Versioning;
using Domain.Helpers;
using Domain.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý phiếu nhập hàng.
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class InputController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách phiếu nhập (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InputResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInputs(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetInputsListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập đã bị xóa (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet("deleted")]
    [ProducesResponseType(typeof(PagedResult<InputResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedInputs(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedInputsListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của phiếu nhập.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInputById(int id, CancellationToken cancellationToken)
    {
        var query = new GetInputByIdQuery(id);
        var (data, error) = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập theo nhà cung cấp.
    /// </summary>
    [HttpGet("by-supplier/{supplierId:int}")]
    [ProducesResponseType(typeof(PagedResult<InputResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInputsBySupplierId(
        int supplierId,
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetInputsBySupplierIdQuery(supplierId, sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Tạo phiếu nhập mới.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateInput(
        [FromBody] CreateInputRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateInputCommand>();
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return CreatedAtAction(nameof(GetInputById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Cập nhật phiếu nhập.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInput(
        int id,
        [FromBody] UpdateInputRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateInputCommand>() with { Id = id };
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return Ok(response);
    }

    /// <summary>
    /// Cập nhật trạng thái của phiếu nhập.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInputStatus(
        int id,
        [FromBody] UpdateInputStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateInputStatusCommand>() with { Id = id };
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return Ok(response);
    }

    /// <summary>
    /// Cập nhật trạng thái của nhiều phiếu nhập cùng lúc.
    /// </summary>
    [HttpPatch("status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateManyInputStatus(
        [FromBody] UpdateManyInputStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateManyInputStatusCommand>();
        await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return NoContent();
    }

    /// <summary>
    /// Xóa phiếu nhập.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInput(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteInputCommand(id);
        await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return NoContent();
    }

    /// <summary>
    /// Xóa nhiều phiếu nhập cùng lúc.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteManyInputs(
        [FromBody] DeleteManyInputsRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManyInputsCommand>();
        await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return NoContent();
    }

    /// <summary>
    /// Khôi phục phiếu nhập đã bị xóa.
    /// </summary>
    [HttpPost("{id:int}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreInput(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreInputCommand(id);
        await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return NoContent();
    }

    /// <summary>
    /// Khôi phục nhiều phiếu nhập đã bị xóa cùng lúc.
    /// </summary>
    [HttpPost("restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RestoreManyInputs(
        [FromBody] RestoreManyInputsRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyInputsCommand>();
        await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return NoContent();
    }
}
```

#### StatisticalController Example

```csharp
using Application.Features.Statistical.Queries.GetDailyRevenue;
using Application.Features.Statistical.Queries.GetDashboardStats;
using Application.Features.Statistical.Queries.GetMonthlyRevenueProfit;
using Application.Features.Statistical.Queries.GetOrderStatusCounts;
using Application.Features.Statistical.Queries.GetProductReportLastMonth;
using Application.Features.Statistical.Queries.GetProductStockAndPrice;
using Application.Interfaces.Repositories.Statistical;
using Asp.Versioning;
using Domain.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1;

/// <summary>
/// API thống kê và báo cáo.
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class StatisticalController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy doanh thu theo ngày trong khoảng thời gian xác định.
    /// </summary>
    /// <param name="days">Số ngày tính từ hiện tại trở về trước</param>
    [HttpGet("daily-revenue")]
    [ProducesResponseType(typeof(IEnumerable<DailyRevenueDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyRevenue(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDailyRevenueQuery(days);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy các chỉ số tổng hợp cho Dashboard.
    /// </summary>
    [HttpGet("dashboard-stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
    {
        var query = new GetDashboardStatsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy doanh thu và lợi nhuận theo tháng.
    /// </summary>
    /// <param name="months">Số tháng tính từ hiện tại trở về trước</param>
    [HttpGet("monthly-revenue-profit")]
    [ProducesResponseType(typeof(IEnumerable<MonthlyRevenueProfitDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyRevenueProfit(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMonthlyRevenueProfitQuery(months);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy số lượng đơn hàng theo từng trạng thái.
    /// </summary>
    [HttpGet("order-status-counts")]
    [ProducesResponseType(typeof(IEnumerable<OrderStatusCountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderStatusCounts(CancellationToken cancellationToken)
    {
        var query = new GetOrderStatusCountsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy báo cáo sản phẩm của tháng trước.
    /// </summary>
    [HttpGet("product-report-last-month")]
    [ProducesResponseType(typeof(IEnumerable<ProductReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductReportLastMonth(CancellationToken cancellationToken)
    {
        var query = new GetProductReportLastMonthQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy giá và tồn kho của một sản phẩm cụ thể.
    /// </summary>
    [HttpGet("product-stock-price/{variantId:int}")]
    [ProducesResponseType(typeof(ProductStockPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductStockAndPrice(
        int variantId,
        CancellationToken cancellationToken)
    {
        var query = new GetProductStockAndPriceQuery(variantId);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        if(result is null)
        {
            return NotFound(new ErrorResponse
            {
                StatusCode = 404,
                Message = $"Không tìm thấy sản phẩm có ID {variantId}."
            });
        }
        return Ok(result);
    }
}
```

## ⚠️ Lưu Ý Quan Trọng

### 1. Về UpdateOutputStatus và UpdateManyOutputStatus

- **BẮT BUỘC** phải validate status transition bằng `OrderStatusTransitions.IsTransitionAllowed()`
- **BẮT BUỘC** phải check stock trước khi chuyển sang `completed`
- **BẮT BUỘC** phải gọi `ProcessCOGSForCompletedOrderAsync()` khi chuyển sang `completed`

### 2. Về Transaction trong UpdateMany

- Khi update many, nếu có **1 item fail** thì phải **rollback toàn bộ**
- Validate TẤT CẢ items trước khi bắt đầu update bất kỳ item nào
- Ví dụ:

```csharp
// ✅ ĐÚNG
var allInputs = await readRepository.GetByIdAsync(request.Ids, cancellationToken);
if(allInputs.Count() != request.Ids.Count)
{
    throw new InvalidOperationException("...");
}
// Validate ALL first
foreach(var input in allInputs)
{
    // Validate...
}
// Then update ALL
foreach(var input in allInputs)
{
    input.StatusId = request.StatusId;
}
await unitOfWork.SaveChangesAsync(cancellationToken);

// ❌ SAI
foreach(var id in request.Ids)
{
    var input = await readRepository.GetByIdAsync(id, cancellationToken);
    input.StatusId = request.StatusId;
    await unitOfWork.SaveChangesAsync(cancellationToken); // ❌ Lưu từng cái một
}
```

### 3. Về Supplier TotalInput

- Cần query với LEFT JOIN để tính tổng tiền nhập từ InputReceipts
- Chỉ tính các Input có `StatusId == 'finished'`
- Formula: `SUM(InputInfo.Count * InputInfo.InputPrice)` cho mỗi Input, rồi SUM tất cả

Recommend: Tạo một method riêng trong `ISupplierReadRepository`:

```csharp
Task<IEnumerable<SupplierWithTotalDto>> GetSuppliersWithTotalInputAsync(
    CancellationToken cancellationToken,
    DataFetchMode mode = DataFetchMode.ActiveOnly);
```

### 4. Về Statistical Queries

- Tất cả đều **chỉ cần Query**, không cần Command
- Repository đã implement đầy đủ logic, Handler chỉ cần gọi repository
- Example:

```csharp
public sealed class GetDailyRevenueQueryHandler(
    IStatisticalReadRepository repository) : IRequestHandler<GetDailyRevenueQuery, IEnumerable<DailyRevenueDto>>
{
    public Task<IEnumerable<DailyRevenueDto>> Handle(
        GetDailyRevenueQuery request,
        CancellationToken cancellationToken)
    {
        return repository.GetDailyRevenueAsync(
            request.Days,
            cancellationToken);
    }
}
```

### 5. Validation (FluentValidation - Optional)

Nếu muốn thêm Validators, tham khảo pattern trong dự án hiện có.
Ví dụ:

```csharp
using FluentValidation;

namespace Application.Features.Inputs.Commands.CreateInput;

public sealed class CreateInputCommandValidator : AbstractValidator<CreateInputCommand>
{
    public CreateInputCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0)
            .When(x => x.SupplierId.HasValue)
            .WithMessage("Mã nhà cung cấp không hợp lệ.");

        RuleFor(x => x.StatusId)
            .Must(s => InputStatus.IsValid(s))
            .When(x => !string.IsNullOrWhiteSpace(x.StatusId))
            .WithMessage("Trạng thái không hợp lệ.");

        RuleFor(x => x.Products)
            .NotEmpty()
            .WithMessage("Phiếu nhập phải có ít nhất một sản phẩm.");

        RuleForEach(x => x.Products)
            .ChildRules(product =>
            {
                product.RuleFor(p => p.ProductId)
                    .GreaterThan(0)
                    .WithMessage("Mã sản phẩm không hợp lệ.");

                product.RuleFor(p => p.Count)
                    .GreaterThan((short)0)
                    .WithMessage("Số lượng phải lớn hơn 0.");

                product.RuleFor(p => p.InputPrice)
                    .GreaterThanOrEqualTo(0L)
                    .WithMessage("Giá nhập không được âm.");
            });
    }
}
```

## 🔄 Next Steps

1. **Tạo tất cả MediatR Handlers** theo pattern đã cung cấp
2. **Tạo 3 Controllers**: InputController, OutputController, StatisticalController
3. **Cập nhật SupplierReadRepository** để tính TotalInput
4. **Cập nhật GetSuppliersListQueryHandler** để sử dụng method mới
5. **Test từng API endpoint** một cách kỹ lưỡng
6. **Chạy `dotnet build`** để đảm bảo không có lỗi
7. **Chạy `dotnet test`** (nếu có test project)

## 🎯 Testing Checklist

### Input APIs

- [ ] GET /api/v1/input - List inputs with pagination
- [ ] GET /api/v1/input/deleted - List deleted inputs
- [ ] GET /api/v1/input/{id} - Get input by ID
- [ ] GET /api/v1/input/by-supplier/{supplierId} - Get inputs by supplier
- [ ] POST /api/v1/input - Create input (validate supplier active & product for-sale)
- [ ] PUT /api/v1/input/{id} - Update input (with detail sync logic)
- [ ] PATCH /api/v1/input/{id}/status - Update input status
- [ ] PATCH /api/v1/input/status - Update many input status (transaction safety)
- [ ] DELETE /api/v1/input/{id} - Delete input (soft delete)
- [ ] DELETE /api/v1/input - Delete many inputs (transaction safety)
- [ ] POST /api/v1/input/{id}/restore - Restore deleted input
- [ ] POST /api/v1/input/restore - Restore many deleted inputs

### Output APIs

- [ ] GET /api/v1/output - List outputs with pagination
- [ ] GET /api/v1/output/deleted - List deleted outputs
- [ ] GET /api/v1/output/{id} - Get output by ID
- [ ] POST /api/v1/output - Create output (validate product for-sale)
- [ ] PUT /api/v1/output/{id} - Update output (with detail sync logic)
- [ ] PATCH /api/v1/output/{id}/status - Update output status (**TEST TRANSITION RULES**)
- [ ] PATCH /api/v1/output/status - Update many output status (transaction safety)
- [ ] DELETE /api/v1/output/{id} - Delete output (soft delete)
- [ ] DELETE /api/v1/output - Delete many outputs (transaction safety)
- [ ] POST /api/v1/output/{id}/restore - Restore deleted output
- [ ] POST /api/v1/output/restore - Restore many deleted outputs

### Statistical APIs

- [ ] GET /api/v1/statistical/daily-revenue?days=7
- [ ] GET /api/v1/statistical/dashboard-stats
- [ ] GET /api/v1/statistical/monthly-revenue-profit?months=12
- [ ] GET /api/v1/statistical/order-status-counts
- [ ] GET /api/v1/statistical/product-report-last-month
- [ ] GET /api/v1/statistical/product-stock-price/{variantId}

### Supplier (Updated)

- [ ] GET /api/v1/supplier - Verify TotalInput is calculated correctly

## 📚 Tài Liệu Tham Khảo

- Clean Architecture: https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- CQRS Pattern: https://martinfowler.com/bliki/CQRS.html
- MediatR Documentation: https://github.com/jbogard/MediatR
- Entity Framework Core: https://docs.microsoft.com/en-us/ef/core/
- Sieve (Filtering/Sorting/Pagination): https://github.com/Biarity/Sieve

## ⚡ Quick Commands

```bash
# Build project
dotnet build

# Run project
dotnet run --project WebAPI

# Run tests (if available)
dotnet test

# Create migration (if needed)
dotnet ef migrations add <MigrationName> --project Infrastructure --startup-project WebAPI

# Update database
dotnet ef database update --project Infrastructure --startup-project WebAPI
```

---

**Lưu ý cuối cùng:** Đây là một dự án lớn với hơn 150 files cần tạo. Hãy làm từng phần nhỏ, test kỹ trước khi chuyển sang phần tiếp theo. Chúc bạn thành công! 🚀
