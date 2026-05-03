param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName
)

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Dual Migration Creator" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

if ([string]::IsNullOrWhiteSpace($MigrationName))
{
    Write-Host "ERROR: Migration name cannot be empty!" -ForegroundColor Red
    exit 1
}

Write-Host "Migration Name: $MigrationName" -ForegroundColor Yellow
Write-Host ""

Write-Host "[0/3] Checking for existing migrations in this branch..." -ForegroundColor Cyan
$baseBranch = "origin/main"
$null = git rev-parse --verify $baseBranch 2>$null
if ($LASTEXITCODE -ne 0) {
    $baseBranch = "origin/master"
}

$mergeBase = git merge-base "$baseBranch" HEAD 2>$null
if ($LASTEXITCODE -eq 0)
{
$newMigrationsFiles = @(git diff --name-only --diff-filter=A "$baseBranch" HEAD | Select-String "Migrations/" | Select-String "\.cs$" | Select-String -NotMatch "\.Designer\.cs" | Select-String -NotMatch "ModelSnapshot\.cs")

$uniqueMigrations = $newMigrationsFiles | ForEach-Object {
    $fileName = Split-Path $_.ToString() -Leaf
    if ($fileName -match "^\d+_(.+)\.cs$") { $Matches[1] }
} | Select-Object -Unique

$migrationCount = 0
if ($uniqueMigrations) { $migrationCount = $uniqueMigrations.Count }

    if ($migrationCount -ge 1)
    {
        Write-Host "ERROR: Only 1 migration per branch is allowed!" -ForegroundColor Red
        Write-Host "You already have $migrationCount migration(s) in this branch relative to $baseBranch." -ForegroundColor Red
        Write-Host ""
        Write-Host "Details of migrations found in this branch (grouped by provider):" -ForegroundColor Yellow

        $groups = $newMigrationsFiles | Group-Object { Split-Path $_.ToString() -Parent }
        foreach ($group in $groups) {
            $folderName = Split-Path $group.Name -Leaf
            Write-Host "  Provider: $folderName" -ForegroundColor Cyan
            foreach ($file in $group.Group) {
                Write-Host "    - $(Split-Path $file.ToString() -Leaf)" -ForegroundColor Gray
            }
        }

        Write-Host ""
        Write-Host "MIGRATION CONFLICT / SQUASH GUIDE:" -ForegroundColor Magenta
        Write-Host "----------------------------------" -ForegroundColor Magenta
        Write-Host "If you have multiple migrations in this branch, you must combine them into one:" -ForegroundColor Gray
        Write-Host "  1. Undo the locally created migrations (repeat for each migration):" -ForegroundColor White
        Write-Host "     - dotnet ef migrations remove --context ApplicationDBContext --project Infrastructure --startup-project WebAPI" -ForegroundColor Gray
        Write-Host "     - dotnet ef migrations remove --context MySqlDbContext --project Infrastructure --startup-project WebAPI" -ForegroundColor Gray
        Write-Host "     - dotnet ef migrations remove --context PostgreSqlDbContext --project Infrastructure --startup-project WebAPI" -ForegroundColor Gray
        Write-Host "  2. Pull/Merge latest code from the base branch ($baseBranch)." -ForegroundColor White
        Write-Host "  3. After resolving any code conflicts, rerun this script: .\add-migration.ps1 -MigrationName <New_Name>" -ForegroundColor White
        Write-Host "  4. Ensure ModelSnapshot is clean before creating new migrations." -ForegroundColor White
        Write-Host ""
        Write-Host "Please combine your changes into a single migration before proceeding." -ForegroundColor Red
        exit 1
    }
    Write-Host "SUCCESS: No existing migrations found in this branch. Proceeding..." -ForegroundColor Green
}
else
{
    Write-Host "WARNING: Could not find base branch ($baseBranch). Skipping migration count check." -ForegroundColor Yellow
}
Write-Host ""

Write-Host "[1/2] Creating SQL Server Migration (local)..." -ForegroundColor Cyan
dotnet ef migrations add $MigrationName --context ApplicationDBContext --project Infrastructure --startup-project WebAPI

if ($LASTEXITCODE -ne 0)
{
    Write-Host ""
    Write-Host "ERROR: Failed to create SQL Server migration!" -ForegroundColor Red
    exit 1
}

Write-Host "SUCCESS: SQL Server migration created" -ForegroundColor Green
Write-Host ""

Write-Host "[2/3] Creating MySQL Migration (production legacy)..." -ForegroundColor Cyan
dotnet ef migrations add $MigrationName --context MySqlDbContext --output-dir MySqlMigrations --project Infrastructure --startup-project WebAPI

if ($LASTEXITCODE -ne 0)
{
    Write-Host ""
    Write-Host "ERROR: Failed to create MySQL migration!" -ForegroundColor Red
    exit 1
}

Write-Host "SUCCESS: MySQL migration created" -ForegroundColor Green
Write-Host ""

Write-Host "[3/3] Creating PostgreSql Migration (production)..." -ForegroundColor Cyan
dotnet ef migrations add $MigrationName --context PostgreSqlDbContext --output-dir PostgreSqlMigrations --project Infrastructure --startup-project WebAPI

if ($LASTEXITCODE -ne 0)
{
    Write-Host ""
    Write-Host "ERROR: Failed to create PostgreSql migration!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Other migrations were created, but PostgreSql migration failed." -ForegroundColor Yellow
    Write-Host "You can try to create PostgreSql migration manually:" -ForegroundColor Yellow
    Write-Host "  dotnet ef migrations add $MigrationName --context PostgreSqlDbContext --output-dir PostgreSqlMigrations --project Infrastructure --startup-project WebAPI" -ForegroundColor White
    exit 1
}

Write-Host "SUCCESS: PostgreSql migration created" -ForegroundColor Green
Write-Host ""

Write-Host "==================================" -ForegroundColor Green
Write-Host "COMPLETED!" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Green
Write-Host ""
Write-Host "Created migrations:" -ForegroundColor Cyan
Write-Host "  - SQL Server: Infrastructure/Migrations/$MigrationName..." -ForegroundColor White
Write-Host "  - MySQL:      Infrastructure/MySqlMigrations/$MigrationName..." -ForegroundColor White
Write-Host "  - PostgreSql: Infrastructure/PostgreSqlMigrations/$MigrationName..." -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review the migrations you just created" -ForegroundColor White
Write-Host "  2. Run: dotnet ef database update --context ApplicationDBContext --project Infrastructure --startup-project WebAPI (to update local DB)" -ForegroundColor White
Write-Host "  3. Commit and push to master -> GitHub Actions will auto-deploy!" -ForegroundColor White
Write-Host ""
