using ContosoUniversity.Models;
using Microsoft.Extensions.Logging;

namespace ContosoUniversity.Services
{
    /// <summary>
    /// Notification service implementation with in-memory storage.
    /// MSMQ (System.Messaging) is not supported in .NET 8.
    /// Future enhancement: integrate with Azure Service Bus for production scenarios.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IConfiguration _configuration;
        private static readonly List<Notification> _notifications = new();
        private readonly bool _enabled;

        public NotificationService(ILogger<NotificationService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _enabled = _configuration.GetValue<bool>("NotificationSettings:Enabled", false);
        }

        public void SendNotification(string entityType, string entityId, EntityOperation operation, string? userName = null)
        {
            SendNotification(entityType, entityId, null, operation, userName);
        }

        public void SendNotification(string entityType, string entityId, string? entityDisplayName, EntityOperation operation, string? userName = null)
        {
            if (!_enabled)
            {
                _logger.LogInformation("Notification service is disabled. Skipping notification.");
                return;
            }

            try
            {
                var notification = new Notification
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Operation = operation.ToString(),
                    Message = GenerateMessage(entityType, entityId, entityDisplayName, operation),
                    CreatedAt = DateTime.Now,
                    CreatedBy = userName ?? "System",
                    IsRead = false
                };

                lock (_notifications)
                {
                    _notifications.Add(notification);
                }

                _logger.LogInformation("Notification sent: {Message}", notification.Message);

                // TODO: Future enhancement - integrate with Azure Service Bus
                // var serviceBusConnectionString = _configuration.GetValue<string>("NotificationSettings:ServiceBusConnectionString");
                // var queueName = _configuration.GetValue<string>("NotificationSettings:QueueName");
                // if (!string.IsNullOrEmpty(serviceBusConnectionString))
                // {
                //     // Send to Azure Service Bus
                // }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification");
            }
        }

        public Notification? ReceiveNotification()
        {
            if (!_enabled)
            {
                return null;
            }

            try
            {
                lock (_notifications)
                {
                    if (_notifications.Count > 0)
                    {
                        var notification = _notifications[0];
                        _notifications.RemoveAt(0);
                        return notification;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to receive notification");
                return null;
            }
        }

        public void MarkAsRead(int notificationId)
        {
            // TODO: When notifications are persisted to database, update the IsRead status
            _logger.LogInformation("Mark notification {NotificationId} as read", notificationId);
        }

        private string GenerateMessage(string entityType, string entityId, string? entityDisplayName, EntityOperation operation)
        {
            var displayText = !string.IsNullOrWhiteSpace(entityDisplayName) 
                ? $"{entityType} '{entityDisplayName}'" 
                : $"{entityType} (ID: {entityId})";

            return operation switch
            {
                EntityOperation.CREATE => $"New {displayText} has been created",
                EntityOperation.UPDATE => $"{displayText} has been updated",
                EntityOperation.DELETE => $"{displayText} has been deleted",
                _ => $"{displayText} operation: {operation}"
            };
        }
    }
}
