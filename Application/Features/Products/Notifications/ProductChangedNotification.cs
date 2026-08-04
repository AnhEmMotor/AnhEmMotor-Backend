using MediatR;

namespace Application.Features.Products.Notifications;

public record ProductChangedNotification(int ProductId) : INotification;
