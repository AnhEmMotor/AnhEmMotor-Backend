using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Infrastructure.Services.Ai;

public interface IPythonEnvService
{
    public Task<string> GetPythonPathAsync(string sidecarDir);
}

public class PythonEnvService(ILogger<PythonEnvService> logger) : IPythonEnvService
{
    public async Task<string> GetPythonPathAsync(string sidecarDir)
    {
        var isWindows = OperatingSystem.IsWindows();
        var venvDir = Path.Combine(sidecarDir, ".venv");
        var pythonExe = isWindows
            ? Path.Combine(venvDir, "Scripts", "python.exe")
            : Path.Combine(venvDir, "bin", "python3");
        var pipExe = isWindows ? Path.Combine(venvDir, "Scripts", "pip.exe") : Path.Combine(venvDir, "bin", "pip3");
        if (!Directory.Exists(venvDir) || !File.Exists(pythonExe))
        {
            var basePython = isWindows ? "python" : "python3";
            await RunCommandAsync(basePython, $"-m venv \"{venvDir}\"", sidecarDir);
        }
        var reqFile = Path.Combine(sidecarDir, "requirements.txt");
        if (File.Exists(reqFile) && File.Exists(pipExe))
        {
            await RunCommandAsync(pipExe, $"install -r \"{reqFile}\"", sidecarDir);
        }
        return pythonExe;
    }

    private async Task RunCommandAsync(string fileName, string args, string workingDir)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        try
        {
            using var process = Process.Start(startInfo);
            if (process != null)
            {
                process.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data != null && e.Data.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
                        logger.LogWarning("[PythonEnv-Err] {Msg}", e.Data);
                };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();
                if (process.ExitCode != 0)
                {
                    logger.LogWarning(
                        "[PythonEnv] Lệnh '{Cmd} {Args}' kết thúc với mã lỗi {Code}",
                        fileName,
                        args,
                        process.ExitCode);
                }
            }
        } catch (Exception ex)
        {
            logger.LogError(ex, "[PythonEnv] Không thể chạy lệnh '{Cmd} {Args}'.", fileName, args);
            throw;
        }
    }
}
