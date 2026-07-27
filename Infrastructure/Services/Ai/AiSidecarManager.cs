using Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Infrastructure.Services.Ai;

public class AiSidecarManager(
    IPythonEnvService pythonEnv,
    IConfiguration config,
    ILogger<AiSidecarManager> logger,
    IServer server,
    IHostApplicationLifetime lifetime) : IHostedService, IAiSidecarUrlProvider
{
    private Process? _sidecarProcess;

    private string _sidecarUrl = "http://127.0.0.1:8000";

    public string GetSidecarUrl() => _sidecarUrl;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        lifetime.ApplicationStarted.Register(() => Task.Run(async () => await StartSidecarProcessAsync()));
        return Task.CompletedTask;
    }

    private async Task StartSidecarProcessAsync()
    {
        var port = GetFreePort();
        // Dùng thẳng 127.0.0.1 thay vì "localhost" để khớp với địa chỉ uvicorn bind (tránh việc
        // "localhost" phân giải ra ::1 rồi không kết nối được).
        _sidecarUrl = $"http://127.0.0.1:{port}";
        var searchPaths = new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() };
        string? sidecarDir = null;
        foreach (var startPath in searchPaths)
        {
            var checkDir = startPath;
            while (!string.IsNullOrEmpty(checkDir))
            {
                var potential = Path.Combine(checkDir, "AISidecar");
                if (Directory.Exists(potential))
                {
                    sidecarDir = potential;
                    break;
                }
                var parent = Directory.GetParent(checkDir);
                if (parent == null || parent.FullName == checkDir)
                    break;
                checkDir = parent.FullName;
            }
            if (sidecarDir != null)
                break;
        }
        if (sidecarDir == null)
        {
            logger.LogError(
                "[AiSidecar] Không tìm thấy thư mục AISidecar tại {BaseDir} hoặc các thư mục cha.",
                AppContext.BaseDirectory);
            return;
        }
        var pythonExe = await pythonEnv.GetPythonPathAsync(sidecarDir);
        var appDir = Path.Combine(sidecarDir, "app");
        if (!Directory.Exists(appDir))
        {
            logger.LogError("[AiSidecar] Không tìm thấy thư mục app tại {Path}", appDir);
            return;
        }
        var startInfo = new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = $"-m uvicorn app.main:app --host 127.0.0.1 --port {port} --log-level warning",
            WorkingDirectory = sidecarDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        var backendUrl = GetInternalBackendUrl();
        startInfo.EnvironmentVariables["BACKEND_URL"] = $"{backendUrl}/api";
        startInfo.EnvironmentVariables["BACKEND_INTERNAL_SECRET"] = config["Jwt:Key"] ?? string.Empty;
        startInfo.EnvironmentVariables["PORT"] = port.ToString();
        startInfo.EnvironmentVariables["PYTHONPATH"] = sidecarDir;
        startInfo.EnvironmentVariables["PYTHONUNBUFFERED"] = "1";
        var isLangSmithEnabled = config.GetValue<bool>("AISetup:LangSmithTracing");
        if (isLangSmithEnabled)
        {
            startInfo.EnvironmentVariables["LANGCHAIN_TRACING_V2"] = "true";
            startInfo.EnvironmentVariables["LANGCHAIN_ENDPOINT"] = "https://api.smith.langchain.com";
            startInfo.EnvironmentVariables["LANGCHAIN_PROJECT"] = "AnhEmMotor";
            startInfo.EnvironmentVariables["LANGCHAIN_API_KEY"] = config["AISetup:LangSmithApiKey"] ?? string.Empty;
        }
        startInfo.EnvironmentVariables["AI_PROVIDER"] = config["AISetup:Provider"] ?? "Gemini";
        startInfo.EnvironmentVariables["AI_API_ENDPOINT"] = config["AISetup:ApiEndpoint"] ?? string.Empty;
        startInfo.EnvironmentVariables["API_KEY"] = config["AISetup:ApiKey"] ?? string.Empty;
        startInfo.EnvironmentVariables["MODEL"] = config["AISetup:Model"] ?? string.Empty;
        startInfo.EnvironmentVariables["QDRANT_URL"] = config["AISetup:QdrantUrl"] ?? string.Empty;
        startInfo.EnvironmentVariables["QDRANT_API_KEY"] = config["AISetup:QdrantApiKey"] ?? string.Empty;
        startInfo.EnvironmentVariables["POSTGRES_URL"] = config.GetConnectionString("PostgreSql") ?? string.Empty;
        try
        {
            _sidecarProcess = new Process { StartInfo = startInfo };
            _sidecarProcess.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    logger.LogInformation("[Python-Sidecar] {Msg}", e.Data);
            };
            _sidecarProcess.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    logger.LogWarning("[Python-Sidecar-Err] {Msg}", e.Data);
            };
            _sidecarProcess.Start();
            _sidecarProcess.BeginOutputReadLine();
            _sidecarProcess.BeginErrorReadLine();
        } catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[AiSidecar] Lỗi khi khởi chạy AI Sidecar. Vui lòng đảm bảo 'python' đã được cài đặt và có trong PATH.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_sidecarProcess != null && !_sidecarProcess.HasExited)
        {
            try
            {
                _sidecarProcess.Kill(true);
            } catch (Exception ex)
            {
                logger.LogWarning(ex, "[AiSidecar] Lỗi khi đóng tiến trình Sidecar.");
            }
        }
        return Task.CompletedTask;
    }

    private string GetInternalBackendUrl()
    {
        var addressFeature = server.Features.Get<IServerAddressesFeature>();
        if (addressFeature != null && addressFeature.Addresses.Count > 0)
        {
            var address = addressFeature.Addresses.FirstOrDefault(a => a.StartsWith("http://")) ??
                addressFeature.Addresses.First();
            return address
                .Replace("*", "localhost")
                .Replace("0.0.0.0", "localhost")
                .Replace("[::]", "localhost");
        }
        return "http://localhost:5000";
    }

    private int GetFreePort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }
}
