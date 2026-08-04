using System.Net.Http.Json;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Constants.Product;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Product;

public class ProductIndexWorker(
    IProductIndexQueue queue,
    IServiceProvider serviceProvider,
    IAiSidecarUrlProvider sidecarUrlProvider,
    HttpClient httpClient,
    IConfiguration config,
    ILogger<ProductIndexWorker> logger) : BackgroundService
{
    private const int MaxBatchSize = 100;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        httpClient.DefaultRequestHeaders.Remove("X-Internal-Secret");
        httpClient.DefaultRequestHeaders.Add("X-Internal-Secret", config["Jwt:Key"] ?? string.Empty);

        var buffer = new List<int>();
        while (!stoppingToken.IsCancellationRequested)
        {
            // Đọc trong tối đa FlushInterval hoặc tới khi đủ MaxBatchSize — dùng CancellationTokenSource
            // để giới hạn thời gian, KHÔNG dùng Task.WhenAny đua với MoveNextAsync (ValueTask của
            // ChannelReader không an toàn khi bị bỏ dở giữa chừng, gây "asynchronous operation has
            // not completed" nếu vòng lặp sau tạo tiếp một MoveNextAsync khác trên cùng reader).
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            timeoutCts.CancelAfter(FlushInterval);
            try
            {
                await foreach (var productId in queue.ReadAllAsync(timeoutCts.Token))
                {
                    buffer.Add(productId);
                    if (buffer.Count >= MaxBatchSize)
                        break;
                }
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
            }

            if (buffer.Count == 0)
                continue;

            var batch = buffer.Distinct().ToList();
            buffer.Clear();
            try
            {
                await IndexBatchAsync(batch, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi index {Count} sản phẩm vào Qdrant", batch.Count);
            }
        }
    }

    private async Task IndexBatchAsync(List<int> productIds, CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var productReadRepository = scope.ServiceProvider.GetRequiredService<IProductReadRepository>();

        var products = await productReadRepository.GetByIdWithVariantsAsync(productIds, ct, DataFetchMode.All)
            .ConfigureAwait(false);

        var items = products.Select(p => new
        {
            productId = p.Id,
            name = p.Name,
            brand = p.Brand?.Name,
            category = p.ProductCategory?.Name,
            description = p.Description ?? p.ShortDescription,
            colors = p.ProductVariants
                .SelectMany(v => v.ProductVariantColors)
                .Select(c => c.ColorName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToList(),
            price = p.ProductVariants.Count > 0 ? p.ProductVariants.Min(v => v.Price) : null,
            // ponytail: đánh dấu còn hàng theo mặc định — bộ lọc tồn kho thật luôn được
            // semantic_product_search bù lại từ SQL (12.5), Qdrant chỉ cần is_active đúng.
            // Nâng cấp khi cần lọc "chỉ còn hàng" chính xác ngay ở tầng vector search.
            inStock = true,
            isActive = p.DeletedAt == null && p.StatusId == ProductStatus.ForSale,
        }).ToList();

        if (items.Count == 0)
            return;

        var url = $"{sidecarUrlProvider.GetSidecarUrl()}/internal/index/products";
        var response = await httpClient.PostAsJsonAsync(url, new { items }, ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Sidecar từ chối index {Count} sản phẩm: {Status}", items.Count, response.StatusCode);
        }
    }
}
