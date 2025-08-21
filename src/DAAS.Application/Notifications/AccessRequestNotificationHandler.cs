using DAAS.Application.Handlers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DAAS.Application.Notifications;

public class AccessRequestNotificationHandler(ILogger<AccessRequestNotificationHandler> logger) 
    : INotificationHandler<AccessRequestDecisionMadeEvent>
{
    private readonly ILogger<AccessRequestNotificationHandler> _logger = logger;

    public async Task Handle(AccessRequestDecisionMadeEvent notification, CancellationToken cancellationToken)
    {
        // Simulate background job for sending notifications
        _logger.LogInformation("📧 Sending notification for access request decision...");
        
        await Task.Delay(1000, cancellationToken); // Simulate work
        
        var status = notification.IsApproved ? "APPROVED" : "REJECTED";
        var message = $"Access Request #{notification.RequestId} has been {status}";
        
        if (!string.IsNullOrEmpty(notification.Comment))
        {
            message += $" with comment: {notification.Comment}";
        }

        // In a real application, this would:
        // - Send email notification
        // - Update notification systems
        // - Log to audit trail
        // - Send push notifications
        // - Update external systems
        
        _logger.LogInformation("✅ Notification sent successfully: {Message}", message);
        _logger.LogInformation("📊 Notification details - RequestId: {RequestId}, UserId: {UserId}, ApproverId: {ApproverId}, Decision: {Decision}", 
            notification.RequestId, notification.UserId, notification.ApproverId, status);
    }
}