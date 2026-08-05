using Application.Features.Products.Notifications;
using Application.Interfaces.Services;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class ChatIndexing
{
    [Fact(DisplayName = "INDEX_01 - ProductChangedNotificationHandler đẩy đúng productId vào IProductIndexQueue")]
    public async Task Handle_EnqueuesProductId()
    {
        var queue = new Mock<IProductIndexQueue>();
        var handler = new ProductChangedNotificationHandler(queue.Object);

        await handler.Handle(new ProductChangedNotification(42), CancellationToken.None);

        queue.Verify(x => x.EnqueueAsync(42, It.IsAny<CancellationToken>()), Times.Once);
    }
}
