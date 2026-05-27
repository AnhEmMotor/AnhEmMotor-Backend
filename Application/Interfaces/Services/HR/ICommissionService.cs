using Application.Common.Models;

namespace Application.Interfaces.Services.HR;

public interface ICommissionService
{
    /// <summary>
    /// Tạm tính hoa hồng (Pending) khi đơn hàng ở trạng thái Processing/Confirmed. Sale nhìn thấy con số để có động lực
    /// chăm sóc khách.
    /// </summary>
    public Task<Result> CalculatePendingCommissionAsync(int outputId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ghi nhận (Confirm) hoa hồng khi đơn hàng chuyển sang Completed. Lưu Snapshot chính sách để đảm bảo tính bất biến
    /// lịch sử.
    /// </summary>
    public Task<Result> CalculateAndRecordCommissionAsync(int outputId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Duyệt chi trả (Paid) hoa hồng — Admin thực hiện cuối tháng.
    /// </summary>
    public Task<Result> MarkCommissionAsPaidAsync(int outputId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hủy bỏ hoa hồng khi đơn hàng bị hủy hoặc trả lại.
    /// </summary>
    public Task<Result> VoidCommissionAsync(int outputId, CancellationToken cancellationToken = default);
}
