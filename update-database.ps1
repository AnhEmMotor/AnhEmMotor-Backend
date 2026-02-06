# Script to update local database after creating migration
# Usage: .\update-database.ps1

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Update Local Database" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Updating SQL Server database (local)..." -ForegroundColor Yellow
Write-Host ""

dotnet ef database update --project Infrastructure --startup-project WebAPI

if ($LASTEXITCODE -eq 0)
{
    Write-Host ""
    Write-Host "SUCCESS: Database updated!" -ForegroundColor Green
    Write-Host ""
}
else
{
    Write-Host ""
    Write-Host "ERROR: Failed to update database!" -ForegroundColor Red
    Write-Host ""
    exit 1
}
