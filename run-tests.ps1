param(
    [switch]$SkipIntegration,
    [switch]$SkipPython
)

$ErrorActionPreference = "Continue"
$root = $PSScriptRoot
$failed = @()
$skipped = @()

Write-Host "`n=== 1/2 .NET Tests (Unit, Controller, Integration) ===" -ForegroundColor Cyan
if (-not $SkipIntegration) {
    $dockerUp = $false
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        docker info *> $null
        $dockerUp = ($LASTEXITCODE -eq 0)
    }
    if (-not $dockerUp) {
        Write-Host "CẢNH BÁO: Docker chưa chạy! Integration tests có thể sẽ thất bại." -ForegroundColor Yellow
    }
    
    # Chạy toàn bộ test trong solution
    dotnet test "$root/AnhEmMotor-Backend.sln" --nologo
    if ($LASTEXITCODE -ne 0) { $failed += ".NET Tests" }
} else {
    Write-Host "Đang bỏ qua Integration Tests, chỉ chạy Unit & Controller tests..." -ForegroundColor Yellow
    dotnet test "$root/UnitTests/UnitTests.csproj" --nologo
    if ($LASTEXITCODE -ne 0) { $failed += "UnitTests" }
    dotnet test "$root/ControllerTests/ControllerTests.csproj" --nologo
    if ($LASTEXITCODE -ne 0) { $failed += "ControllerTests" }
    $skipped += "IntegrationTests (-SkipIntegration)"
}

if (-not $SkipPython) {
    Write-Host "`n=== 2/2 AISidecar tests (Python) ===" -ForegroundColor Cyan
    $python = Join-Path $root "AISidecar/.venv/Scripts/python.exe"
    if (-not (Test-Path $python)) { $python = Join-Path $root "AISidecar/.venv/bin/python" }

    if (-not (Test-Path $python)) {
        Write-Host "Không tìm thấy venv của AISidecar — bỏ qua." -ForegroundColor Yellow
        $skipped += "AISidecar (không có .venv)"
    } else {
        Push-Location (Join-Path $root "AISidecar")
        & $python -m pytest
        if ($LASTEXITCODE -ne 0) { $failed += "AISidecar" }
        Pop-Location
    }
} else {
    Write-Host "`n=== 2/2 AISidecar tests — đã bỏ qua ===" -ForegroundColor Yellow
    $skipped += "AISidecar (-SkipPython)"
}
Write-Host ""
if ($failed.Count -gt 0) {
    Write-Host "THẤT BẠI: $($failed -join ', ')" -ForegroundColor Red
    if ($skipped.Count -gt 0) {
        Write-Host "Chưa chạy: $($skipped -join ', ')" -ForegroundColor Yellow
    }
    exit 1
}

if ($skipped.Count -gt 0) {
    Write-Host "Các nhóm ĐÃ CHẠY đều pass. CHƯA CHẠY: $($skipped -join ', ')" -ForegroundColor Yellow
    exit 0
}
Write-Host "Tất cả test đã pass." -ForegroundColor Green
