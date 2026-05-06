$port = 5000
$process = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
if ($process) {
    Write-Host "Killing process $process using port $port"
    Stop-Process -Id $process -Force -ErrorAction SilentlyContinue
}
dotnet run --project WebAPI/WebAPI.csproj
