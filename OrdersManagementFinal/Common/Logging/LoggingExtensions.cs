using Week4.Common.Logging;

namespace Week4.Common.Logging
{
    public static class LoggingExtensions
    {
            public static void LogOrderCreationMetrics(this ILogger logger, OrderCreationMetrics metrics)
            {
                logger.LogInformation(
                    new EventId(LogEvents.OrderCreationCompleted, nameof(LogEvents.OrderCreationCompleted)),
                    "Order Metrics: [OperationId={OperationId}] Title={Title}, ISBN={ISBN}, OrderCategory={Category}, " +
                    "Validation={ValidationDuration}ms, DB Save={DatabaseSaveDuration}ms, Total={TotalDuration}ms, Success={Success}, Error={Error}",
                    metrics.OperationId,
                    metrics.OrderTitle,
                    metrics.ISBN,
                    metrics.Category,
                    metrics.ValidationDuration.TotalMilliseconds,
                    metrics.DatabaseSaveDuration.TotalMilliseconds,
                    metrics.TotalDuration.TotalMilliseconds,
                    metrics.Success,
                    metrics.ErrorReason ?? "None"
                );
            }
        }
    }

