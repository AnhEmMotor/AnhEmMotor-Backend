# Script cập nhật Migration và khởi chạy ứng dụng AnhEmMotor-Backend

$errorActionPreference = "Stop"

Write-Host "--- Đang bắt đầu quy trình cập nhật Database ---" -ForegroundColor Cyan

try {
    Write-Host "1. Kiểm tra và cài đặt dotnet-ef tool..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef 2>$null
    # Nếu đã cài rồi sẽ báo lỗi, ta bỏ qua lỗi này

    Write-Host "2. Tạo Migration mới cho các thay đổi trong Model..." -ForegroundColor Yellow
    cd "D:\DATN\AnhEmMotor\AnhEmMotor-Backend\WebAPI"
    dotnet ef migrations add UpdateDashboardEntities --project ..\Infrastructure\Infrastructure.csproj --startup-project .

    Write-Host "3. Khởi chạy ứng dụng để tự động apply migration và seed data..." -ForegroundColor Yellow
    dotnet run
}
catch {
    Write-Host "Đã xảy ra lỗi: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Vui lòng kiểm tra lại kết nối database hoặc cấu hình appsettings.json" -ForegroundColor Red
}

Write-Host "--- Quy trình hoàn tất ---" -ForegroundColor Cyan
