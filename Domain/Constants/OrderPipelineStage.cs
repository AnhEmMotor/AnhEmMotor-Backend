namespace AnhEmMotor.Domain.Constants
{
    public enum OrderPipelineStage
    {
        DepositReceived = 0,      // Đã nhận cọc
        ProcessingPaperwork = 1,  // Đang làm biển số/hồ sơ
        ReadyForDelivery = 2,     // Sẵn sàng giao xe
        InTransit = 3,            // Đang vận chuyển
        Delivered = 4             // Đã giao xe
    }
}