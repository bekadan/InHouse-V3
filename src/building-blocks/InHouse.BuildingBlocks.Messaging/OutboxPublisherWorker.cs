using InHouse.BuildingBlocks.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InHouse.BuildingBlocks.Messaging;

public sealed class OutboxPublisherWorker<TDbContext> : BackgroundService
    where TDbContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventBus _eventBus;
    private readonly OutboxPublisherOptions _options;
    private readonly ILogger<OutboxPublisherWorker<TDbContext>> _logger;

    public OutboxPublisherWorker(
        IServiceScopeFactory scopeFactory,
        IEventBus eventBus,
        IOptions<OutboxPublisherOptions> options,
        ILogger<OutboxPublisherWorker<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _eventBus = eventBus;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxPublisherWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxPublisherWorker error.");
            }

            await Task.Delay(_options.PollingInterval, stoppingToken);
        }
    }

    private async Task PublishBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

        var messages = await db.Set<OutboxMessage>()
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
            return;

        foreach (var message in messages)
        {
            if (message.AttemptCount >= _options.MaxRetryCount)
            {
                _logger.LogWarning("Outbox message {Id} exceeded max retries.", message.Id);
                continue;
            }

            try
            {
                var envelope = new EventEnvelope
                {
                    MessageId = message.Id,
                    EventType = message.Type,
                    Payload = message.PayloadJson,
                    Headers = message.HeadersJson,
                    OccurredOnUtc = message.OccurredOnUtc,
                    PublishedOnUtc = DateTime.UtcNow
                };

                await _eventBus.PublishAsync(envelope, cancellationToken);

                message.MarkProcessed(DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed publishing outbox message {Id}", message.Id);
                message.MarkFailed(DateTime.UtcNow, ex.Message);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}