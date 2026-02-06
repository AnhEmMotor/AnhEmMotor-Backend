# Script to automatically create migrations for both SQL Server (local) and MySQL (production)
# Usage: .\add-migration.ps1 "MigrationName"

param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName
)

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Dual Migration Creator" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Validate migration name
if ([string]::IsNullOrWhiteSpace($MigrationName))
{
    Write-Host "ERROR: Migration name cannot be empty!" -ForegroundColor Red
    exit 1
}

Write-Host "Migration Name: $MigrationName" -ForegroundColor Yellow
Write-Host ""

# Create SQL Server Migration (for local development)
Write-Host "[1/2] Creating SQL Server Migration (local)..." -ForegroundColor Cyan
dotnet ef migrations add $MigrationName --project Infrastructure --startup-project WebAPI

if ($LASTEXITCODE -ne 0)
{
    Write-Host ""
    Write-Host "ERROR: Failed to create SQL Server migration!" -ForegroundColor Red
    exit 1
}

Write-Host "SUCCESS: SQL Server migration created" -ForegroundColor Green
Write-Host ""

# Create MySQL Migration (for production)
Write-Host "[2/2] Creating MySQL Migration (production)..." -ForegroundColor Cyan
dotnet ef migrations add $MigrationName --context MySqlDbContext --output-dir MySqlMigrations --project Infrastructure --startup-project WebAPI

if ($LASTEXITCODE -ne 0)
{
    Write-Host ""
    Write-Host "ERROR: Failed to create MySQL migration!" -ForegroundColor Red
    Write-Host ""
    Write-Host "SQL Server migration was created, but MySQL migration failed." -ForegroundColor Yellow
    Write-Host "You can try to create MySQL migration manually:" -ForegroundColor Yellow
    Write-Host "  dotnet ef migrations add $MigrationName --context MySqlDbContext --output-dir MySqlMigrations --project Infrastructure --startup-project WebAPI" -ForegroundColor White
    exit 1
}

Write-Host "SUCCESS: MySQL migration created" -ForegroundColor Green
Write-Host ""

# Success
Write-Host "==================================" -ForegroundColor Green
Write-Host "COMPLETED!" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Green
Write-Host ""
Write-Host "Created migrations:" -ForegroundColor Cyan
Write-Host "  - SQL Server: Infrastructure/Migrations/$MigrationName..." -ForegroundColor White
Write-Host "  - MySQL:      Infrastructure/MySqlMigrations/$MigrationName..." -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review the migrations you just created" -ForegroundColor White
Write-Host "  2. Run: dotnet ef database update --project Infrastructure --startup-project WebAPI (to update local DB)" -ForegroundColor White
Write-Host "  3. Commit and push to master -> GitHub Actions will auto-deploy!" -ForegroundColor White
Write-Host ""
