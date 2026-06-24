namespace Domain.Constants
{
    public enum OrderPipelineStage
    {
        DepositReceived = 0,
        ProcessingPaperwork = 1,
        ReadyForDelivery = 2,
        InTransit = 3,
        Delivered = 4
    }
}
