using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Common.Models;
using Application.Common.Models.Ga4;
using Application.Interfaces.Services.Analytics;
using Infrastructure.Configurations.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Analytics;

/// <summary>
/// Đọc chỉ số Google Analytics 4 qua Data API (REST v1beta) bằng Service Account.
/// Credential chỉ tồn tại ở Backend; kết quả được cache ngắn để tiết kiệm quota.
/// </summary>
public class Ga4AnalyticsService(
    IOptions<GoogleAnalytics4Options> options,
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    ILogger<Ga4AnalyticsService> logger) : IGa4AnalyticsService
{
    private const string DataApiBaseUrl = "https://analyticsdata.googleapis.com/v1beta";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string ReadOnlyScope = "https://www.googleapis.com/auth/analytics.readonly";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    // Realtime cần dữ liệu "sống" — chỉ cache 60s để dashboard tự làm mới mỗi phút không tốn quota.
    private static readonly TimeSpan RealtimeCacheDuration = TimeSpan.FromSeconds(60);
    private const int RealtimeWindowMinutes = 30;

    private static readonly string[] MetricNames =
    [
        "sessions",
        "totalUsers",
        "newUsers",
        "activeUsers",
        "screenPageViews",
        "engagementRate",
        "averageSessionDuration",
        "keyEvents"
    ];

    // Realtime API chỉ hỗ trợ tập metric hẹp hơn runReport thường.
    private static readonly string[] RealtimeMetricNames = ["activeUsers", "screenPageViews"];

    private static readonly JsonSerializerOptions PayloadOptions = new(JsonSerializerDefaults.Web);

    private GoogleAnalytics4Options Options => options.Value;

    private readonly SemaphoreSlim _credentialLock = new(1, 1);
    private string? _cachedAccessToken;
    private DateTimeOffset _accessTokenExpiryUtc;

    public bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(Options.PropertyId)
            && Options.ServiceAccount.Count > 0
            && Options.ServiceAccount.ContainsKey("client_email")
            && Options.ServiceAccount.ContainsKey("private_key");
    }

    public async Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> RunReportAsync(
        Ga4ReportRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured())
        {
            return Result<Ga4ReportDto<Ga4DimensionRowDto>>.Failure("Google Analytics 4 chưa được cấu hình trên server.");
        }

        var dimension = string.IsNullOrWhiteSpace(request.Dimension) ? null : request.Dimension.Trim();
        var cacheKey =
            $"ga4:{Options.PropertyId}:{request.StartDate:yyyyMMdd}:{request.EndDate:yyyyMMdd}:{dimension ?? "_total"}:{request.Limit}";
        if (cache.TryGetValue(cacheKey, out Ga4ReportDto<Ga4DimensionRowDto>? cached) && cached is not null)
        {
            return Result<Ga4ReportDto<Ga4DimensionRowDto>>.Success(cached);
        }

        var payload = BuildPayload(request.StartDate, request.EndDate, dimension, request.Limit);
        var response = await PostGa4Async("runReport", payload, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccess)
        {
            return Result<Ga4ReportDto<Ga4DimensionRowDto>>.Failure(response.Error!);
        }

        var rows = ParseRows(response.Value.RootElement, dimension).ToList();
        var dto = new Ga4ReportDto<Ga4DimensionRowDto>
        {
            PropertyId = Options.PropertyId,
            StartDate = request.StartDate.ToString("yyyy-MM-dd"),
            EndDate = request.EndDate.ToString("yyyy-MM-dd"),
            Rows = rows,
            RowCount = rows.Count
        };
        cache.Set(cacheKey, dto, CacheDuration);
        return Result<Ga4ReportDto<Ga4DimensionRowDto>>.Success(dto);
    }

    public async Task<Result<Ga4OverviewDto>> GetOverviewAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var result = await RunReportAsync(
                new Ga4ReportRequest { StartDate = startDate, EndDate = endDate, Limit = 1 },
                cancellationToken)
            .ConfigureAwait(false);
        if (result.IsFailure)
        {
            return Result<Ga4OverviewDto>.Failure(result.Error!);
        }

        var row = result.Value.Rows.FirstOrDefault() ?? new Ga4DimensionRowDto();
        return Result<Ga4OverviewDto>.Success(new Ga4OverviewDto
        {
            StartDate = result.Value.StartDate,
            EndDate = result.Value.EndDate,
            Sessions = row.Sessions,
            TotalUsers = row.TotalUsers,
            NewUsers = row.NewUsers,
            ActiveUsers = row.ActiveUsers,
            ScreenPageViews = row.ScreenPageViews,
            EngagementRate = row.EngagementRate,
            AverageSessionDuration = row.AverageSessionDuration,
            KeyEvents = row.KeyEvents
        });
    }

    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> GetDailySeriesAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken) => RunReportAsync(
        new Ga4ReportRequest { StartDate = startDate, EndDate = endDate, Dimension = "date", Limit = 400 },
        cancellationToken);

    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> GetTopSourcesAsync(
        DateOnly startDate,
        DateOnly endDate,
        int limit,
        CancellationToken cancellationToken) => RunReportAsync(
        new Ga4ReportRequest { StartDate = startDate, EndDate = endDate, Dimension = "sessionSource", Limit = limit },
        cancellationToken);

    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> GetTopPagesAsync(
        DateOnly startDate,
        DateOnly endDate,
        int limit,
        CancellationToken cancellationToken) => RunReportAsync(
        new Ga4ReportRequest { StartDate = startDate, EndDate = endDate, Dimension = "pagePath", Limit = limit },
        cancellationToken);

    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> GetDeviceBreakdownAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken) => RunReportAsync(
        new Ga4ReportRequest { StartDate = startDate, EndDate = endDate, Dimension = "deviceCategory", Limit = 10 },
        cancellationToken);

    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> GetTopPageTitlesAsync(
        DateOnly startDate,
        DateOnly endDate,
        int limit,
        CancellationToken cancellationToken) => RunReportAsync(
        new Ga4ReportRequest { StartDate = startDate, EndDate = endDate, Dimension = "pageTitle", Limit = limit },
        cancellationToken);

    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> GetOperatingSystemBreakdownAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken) => RunReportAsync(
        new Ga4ReportRequest { StartDate = startDate, EndDate = endDate, Dimension = "operatingSystem", Limit = 10 },
        cancellationToken);

    public Task<Result<Ga4ReportDto<Ga4DimensionRowDto>>> GetBrowserBreakdownAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken) => RunReportAsync(
        new Ga4ReportRequest { StartDate = startDate, EndDate = endDate, Dimension = "browser", Limit = 10 },
        cancellationToken);

    /// <summary>
    /// Chỉ số realtime 30 phút qua qua endpoint runRealtimeReport: tổng người dùng/lượt xem,
    /// chuỗi theo phút, thành phần truy cập theo nguồn và phân rã thiết bị. Cache 60 giây.
    /// </summary>
    public async Task<Result<Ga4RealtimeDto>> GetRealtimeAsync(CancellationToken cancellationToken)
    {
        if (!IsConfigured())
        {
            return Result<Ga4RealtimeDto>.Failure("Google Analytics 4 chưa được cấu hình trên server.");
        }

        var cacheKey = $"ga4-realtime:{Options.PropertyId}";
        if (cache.TryGetValue(cacheKey, out Ga4RealtimeDto? cached) && cached is not null)
        {
            return Result<Ga4RealtimeDto>.Success(cached);
        }

        var minuteRanges = new object[]
        {
            new Dictionary<string, int>
            {
                // 0-29 phút trước == cửa sổ 30 phút đầy đủ (endMinutesAgo=0 là phút hiện tại).
                ["startMinutesAgo"] = RealtimeWindowMinutes - 1,
                ["endMinutesAgo"] = 0
            }
        };

        var minutesTask = PostGa4Async(
            "runRealtimeReport",
            BuildRealtimePayload("minutesAgo", minuteRanges),
            cancellationToken);
        var deviceTask = PostGa4Async(
            "runRealtimeReport",
            BuildRealtimePayload("deviceCategory", minuteRanges),
            cancellationToken);
        var sourceTask = PostGa4Async(
            "runRealtimeReport",
            BuildRealtimePayload("city", minuteRanges),
            cancellationToken);

        await Task.WhenAll(minutesTask, deviceTask, sourceTask).ConfigureAwait(false);

        // Chuỗi theo phút là bắt buộc — lỗi thì báo failure để client hiển thị "chưa có dữ liệu".
        if (minutesTask.Result.IsFailure)
        {
            return Result<Ga4RealtimeDto>.Failure(minutesTask.Result.Error!);
        }

        var minuteRows = ParseRealtimeRows(minutesTask.Result.Value.RootElement);
        // minutesAgo là số (0..29) — sort theo số, tránh "10" đứng trước "2" khi so chuỗi.
        minuteRows.Sort((a, b) =>
        {
            var parseA = long.TryParse(a.Label, out var labelA);
            var parseB = long.TryParse(b.Label, out var labelB);
            return parseA && parseB ? labelA.CompareTo(labelB) : string.CompareOrdinal(a.Label, b.Label);
        });

        // Tổng cả cửa sổ 30 phút: ưu tiên khối "totals" GA4 trả kèm; thiếu thì cộng từ phân rã thiết bị.
        var totals = ParseRealtimeTotals(minutesTask.Result.Value.RootElement);
        var deviceRows = deviceTask.Result.IsSuccess ? ParseRealtimeRows(deviceTask.Result.Value.RootElement) : [];
        var sourceRows = sourceTask.Result.IsSuccess ? ParseRealtimeRows(sourceTask.Result.Value.RootElement) : [];

        if (totals is null)
        {
            var baseRows = (IReadOnlyList<Ga4RealtimeRowDto>)deviceRows;
            if (baseRows.Count == 0)
            {
                baseRows = sourceRows.Count > 0 ? sourceRows : minuteRows;
            }

            totals = (baseRows.Sum(r => r.ActiveUsers), baseRows.Sum(r => r.ScreenPageViews));
        }

        if (!deviceTask.Result.IsSuccess)
        {
            logger.LogWarning("GA4 realtime: lấy phân rã thiết bị thất bại — {Error}", deviceTask.Result.Error);
        }

        if (!sourceTask.Result.IsSuccess)
        {
            logger.LogWarning("GA4 realtime: lấy thành phần truy cập thất bại — {Error}", sourceTask.Result.Error);
        }

        deviceRows.Sort((a, b) => b.ActiveUsers.CompareTo(a.ActiveUsers));
        sourceRows.Sort((a, b) => b.ActiveUsers.CompareTo(a.ActiveUsers));

        var dto = new Ga4RealtimeDto
        {
            ActiveUsers = totals.Value.activeUsers,
            ScreenPageViews = totals.Value.screenPageViews,
            ByMinute = minuteRows,
            BySource = sourceRows,
            ByDevice = deviceRows,
            RetrievedAt = DateTime.UtcNow.ToString("O")
        };
        cache.Set(cacheKey, dto, RealtimeCacheDuration);
        return Result<Ga4RealtimeDto>.Success(dto);
    }

    /// <summary>Payload cho runRealtimeReport — metric hẹp + cửa sổ minuteRanges 30 phút.</summary>
    internal static Dictionary<string, object> BuildRealtimePayload(
        string? dimension,
        object[] minuteRanges)
    {
        var payload = new Dictionary<string, object>
        {
            ["metrics"] = RealtimeMetricNames.Select(name => new Dictionary<string, string> { ["name"] = name }).ToArray(),
            ["minuteRanges"] = minuteRanges,
            ["limit"] = 100
        };

        if (dimension is not null)
        {
            payload["dimensions"] = new[] { new Dictionary<string, string> { ["name"] = dimension } };
        }

        return payload;
    }

    private static List<Ga4RealtimeRowDto> ParseRealtimeRows(JsonElement root)
    {
        var result = new List<Ga4RealtimeRowDto>();
        if (!root.TryGetProperty("rows", out var rows) || rows.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var row in rows.EnumerateArray())
        {
            var label = row.TryGetProperty("dimensionValues", out var dv) &&
                dv.ValueKind == JsonValueKind.Array &&
                dv.GetArrayLength() > 0
                ? dv[0].GetProperty("value").GetString() ?? string.Empty
                : string.Empty;
            var metrics = row.TryGetProperty("metricValues", out var mv) && mv.ValueKind == JsonValueKind.Array
                ? mv.EnumerateArray().Select(m => m.GetProperty("value").GetString() ?? "0").ToArray()
                : [];

            result.Add(new Ga4RealtimeRowDto
            {
                Label = label,
                ActiveUsers = ParseLong(metrics, 0),
                ScreenPageViews = ParseLong(metrics, 1)
            });
        }

        return result;
    }

    /// <summary>Đọc khối "totals" của response realtime (tổng trên toàn bộ minuteRanges).</summary>
    private static (long activeUsers, long screenPageViews)? ParseRealtimeTotals(JsonElement root)
    {
        if (!root.TryGetProperty("totals", out var totals) ||
            totals.ValueKind != JsonValueKind.Array ||
            totals.GetArrayLength() == 0)
        {
            return null;
        }

        var metrics = totals[0].TryGetProperty("metricValues", out var mv) && mv.ValueKind == JsonValueKind.Array
            ? mv.EnumerateArray().Select(m => m.GetProperty("value").GetString() ?? "0").ToArray()
            : [];

        return (ParseLong(metrics, 0), ParseLong(metrics, 1));
    }

    internal static Dictionary<string, object> BuildPayload(
        DateOnly startDate,
        DateOnly endDate,
        string? dimension,
        int limit)
    {
        var payload = new Dictionary<string, object>
        {
            ["dateRanges"] = new[]
            {
                new Dictionary<string, string>
                {
                    ["startDate"] = startDate.ToString("yyyy-MM-dd"),
                    ["endDate"] = endDate.ToString("yyyy-MM-dd")
                }
            },
            ["metrics"] = MetricNames.Select(name => new Dictionary<string, string> { ["name"] = name }).ToArray(),
            ["limit"] = Math.Clamp(limit, 1, 1000),
            ["returnPropertyQuota"] = false
        };

        if (dimension is not null)
        {
            payload["dimensions"] = new[] { new Dictionary<string, string> { ["name"] = dimension } };
            if (!string.Equals(dimension, "date", StringComparison.OrdinalIgnoreCase))
            {
                // Sắp theo sessions giảm dần để lấy top; riêng chiều ngày giữ thứ tự thời gian tự nhiên.
                payload["orderBys"] = new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["metric"] = new Dictionary<string, string> { ["metricName"] = "sessions" },
                        ["desc"] = true
                    }
                };
            }
        }

        return payload;
    }

    /// <summary>
    /// POST một payload lên GA4 Data API (method: "runReport" hoặc "runRealtimeReport") và trả JsonDocument.
    /// </summary>
    private async Task<Result<JsonDocument>> PostGa4Async(
        string methodName,
        Dictionary<string, object> payload,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

            var client = httpClientFactory.CreateClient("Ga4Analytics");
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"{DataApiBaseUrl}/properties/{Options.PropertyId}:{methodName}")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload, PayloadOptions), Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var httpResponse = await client.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            var raw = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!httpResponse.IsSuccessStatusCode)
            {
                logger.LogError(
                    "GA4 {Method} thất bại ({StatusCode}): {Body}",
                    methodName,
                    (int)httpResponse.StatusCode,
                    raw);
                var reason = ExtractGoogleErrorMessage(raw);
                return Result<JsonDocument>.Failure(
                    $"Google Analytics trả lỗi {(int)httpResponse.StatusCode}"
                    + (string.IsNullOrWhiteSpace(reason) ? "." : $": {reason}"));
            }

            return Result<JsonDocument>.Success(JsonDocument.Parse(raw));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "GA4: credential hoặc token endpoint có vấn đề.");
            return Result<JsonDocument>.Failure(ex.Message);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException)
        {
            logger.LogError(ex, "Lỗi khi gọi Google Analytics Data API.");
            return Result<JsonDocument>.Failure("Không gọi được Google Analytics Data API. Kiểm tra kết nối mạng/proxy.");
        }
        catch (Exception ex)
        {
            // Hàng rào cuối — không bao giờ để exception lọt ra ngoài gây 500/CRITICAL log.
            logger.LogError(ex, "Lỗi không xác định khi truy vấn Google Analytics.");
            return Result<JsonDocument>.Failure("Lỗi không xác định khi truy vấn Google Analytics.");
        }
    }

    /// <summary>Trích message thật từ body lỗi của Google (ghi rõ lý do 403: thiếu quyền/SERVICE_DISABLED...).</summary>
    private static string? ExtractGoogleErrorMessage(string rawBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            if (doc.RootElement.TryGetProperty("error", out var error) &&
                error.TryGetProperty("message", out var message))
            {
                var text = message.GetString() ?? string.Empty;
                return text.Length > 400 ? text[..400] + "..." : text;
            }
        }
        catch (JsonException)
        {
            // body không phải JSON — bỏ qua, trả lý do chung.
        }

        return null;
    }

    private static IEnumerable<Ga4DimensionRowDto> ParseRows(JsonElement root, string? dimension)
    {
        if (!root.TryGetProperty("rows", out var rows) || rows.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var row in rows.EnumerateArray())
        {
            var dimensions = row.TryGetProperty("dimensionValues", out var dv) && dv.ValueKind == JsonValueKind.Array
                ? dv.EnumerateArray().Select(d => d.GetProperty("value").GetString() ?? string.Empty).ToArray()
                : [];
            var metrics = row.TryGetProperty("metricValues", out var mv) && mv.ValueKind == JsonValueKind.Array
                ? mv.EnumerateArray().Select(m => m.GetProperty("value").GetString() ?? "0").ToArray()
                : [];

            yield return new Ga4DimensionRowDto
            {
                Label = dimension is null ? "Tổng" : dimensions.Length > 0 ? dimensions[0] : string.Empty,
                Sessions = ParseLong(metrics, 0),
                TotalUsers = ParseLong(metrics, 1),
                NewUsers = ParseLong(metrics, 2),
                ActiveUsers = ParseLong(metrics, 3),
                ScreenPageViews = ParseLong(metrics, 4),
                EngagementRate = ParseDouble(metrics, 5),
                AverageSessionDuration = ParseDouble(metrics, 6),
                KeyEvents = ParseDouble(metrics, 7)
            };
        }
    }

    private static long ParseLong(string[] values, int index) =>
        index < values.Length && long.TryParse(values[index], out var value) ? value : 0;

    private static double ParseDouble(string[] values, int index) =>
        index < values.Length &&
        double.TryParse(values[index], System.Globalization.CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;

    /// <summary>
    /// Lấy access token bằng JWT bearer flow tự triển khai (thay cho Google.Apis.Auth) — kiểm soát toàn bộ
    /// request/response nên khi token endpoint trả response lạ (do proxy/tường lửa chặn) ta LOG ĐƯỢC body
    /// thật để chẩn đoán, thay vì lỗi mù "Server response does not contain a JSON object".
    /// Token được cache tới gần hết hạn để không phải ký lại JWT mỗi lần gọi report.
    /// </summary>
    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (_cachedAccessToken is not null && DateTimeOffset.UtcNow < _accessTokenExpiryUtc)
        {
            return _cachedAccessToken;
        }

        await _credentialLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Double-check sau khi chờ khoá — thread khác có thể đã refresh xong.
            if (_cachedAccessToken is not null && DateTimeOffset.UtcNow < _accessTokenExpiryUtc)
            {
                return _cachedAccessToken;
            }

            var (clientEmail, privateKeyPem) = ReadServiceAccountFields();
            var assertion = BuildJwtAssertion(clientEmail, privateKeyPem);

            var client = httpClientFactory.CreateClient("Ga4Analytics");
            using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                    ["assertion"] = assertion
                })
            };

            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(body) || !body.TrimStart().StartsWith('{'))
            {
                logger.LogError(
                    "GA4 token endpoint phản hồi bất thường ({StatusCode}, Content-Type: {ContentType}). Body: {Body}. " +
                    "Nguyên nhân thường gặp: proxy/VPN/phần mềm diệt virus đang chặn https://oauth2.googleapis.com/token.",
                    (int)response.StatusCode,
                    response.Content.Headers.ContentType?.MediaType ?? "unknown",
                    body.Length > 800 ? body[..800] : body);
                throw new InvalidOperationException(
                    $"Không lấy được access token từ Google (HTTP {(int)response.StatusCode}, " +
                    "phản hồi không phải JSON). Thường do proxy/VPN/diệt virus chặn oauth2.googleapis.com — xem log chi tiết.");
            }

            using var document = JsonDocument.Parse(body);
            var accessToken = document.RootElement.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("Token endpoint không trả access_token.");
            var expiresIn = document.RootElement.TryGetProperty("expires_in", out var exp) &&
                exp.TryGetInt32(out var seconds)
                ? seconds
                : 3600;

            _cachedAccessToken = accessToken;
            _accessTokenExpiryUtc = DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, expiresIn - 120));
            return _cachedAccessToken;
        }
        finally
        {
            _credentialLock.Release();
        }
    }

    private (string ClientEmail, string PrivateKeyPem) ReadServiceAccountFields()
    {
        if (!Options.ServiceAccount.TryGetValue("client_email", out var clientEmail) ||
            !Options.ServiceAccount.TryGetValue("private_key", out var privateKeyPem) ||
            string.IsNullOrWhiteSpace(clientEmail) ||
            string.IsNullOrWhiteSpace(privateKeyPem))
        {
            throw new InvalidOperationException(
                "Cấu hình GoogleAnalytics4:ServiceAccount thiếu client_email/private_key. " +
                "Hãy dán nguyên nội dung file khoá JSON vào appsettings.");
        }

        return (clientEmail, privateKeyPem);
    }

    /// <summary>Ký JWT RS256 cho grant_type=jwt-bearer.</summary>
    private string BuildJwtAssertion(string clientEmail, string privateKeyPem)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var header = Base64UrlEncode(JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["alg"] = "RS256",
            ["typ"] = "JWT"
        }, PayloadOptions));
        var claims = Base64UrlEncode(JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["iss"] = clientEmail,
            ["scope"] = ReadOnlyScope,
            ["aud"] = TokenEndpoint,
            ["iat"] = now,
            ["exp"] = now + 3600
        }, PayloadOptions));

        var signingInput = $"{header}.{claims}";
        using var rsa = ImportRsaKey(privateKeyPem);
        // .NET 10 đã đổi tên static property: Pkcs -> Pkcs1
        var signature = rsa.SignData(
            Encoding.ASCII.GetBytes(signingInput),
            HashAlgorithmName.SHA256,
            System.Security.Cryptography.RSASignaturePadding.Pkcs1);
        return $"{signingInput}.{Base64UrlEncode(signature)}";
    }

    private static RSA ImportRsaKey(string pem)
    {
        var base64 = pem
            .Replace("-----BEGIN PRIVATE KEY-----", string.Empty)
            .Replace("-----END PRIVATE KEY-----", string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Trim();
        var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(base64), out _);
        return rsa;
    }

    private static string Base64UrlEncode(string input) =>
        Base64UrlEncode(Encoding.UTF8.GetBytes(input));

    private static string Base64UrlEncode(byte[] bytes)
    {
        var base64 = Convert.ToBase64String(bytes);
        return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
