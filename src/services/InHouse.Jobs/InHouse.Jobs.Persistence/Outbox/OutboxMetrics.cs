using System.Diagnostics.Metrics;

namespace InHouse.Jobs.Persistence.Outbox;

public static class OutboxMetrics
{
    public const string MeterName = "InHouse.Jobs.Outbox";
    private static readonly Meter Meter = new(MeterName);

    // Counters
    public static readonly Counter<long> PublishedCount =
        Meter.CreateCounter<long>("outbox_published_total");

    public static readonly Counter<long> FailedCount =
        Meter.CreateCounter<long>("outbox_failed_total");

    public static readonly Counter<long> DeadLetteredCount =
        Meter.CreateCounter<long>("outbox_deadlettered_total");

    public static readonly Histogram<double> PublishDurationMs =
        Meter.CreateHistogram<double>("outbox_publish_duration_ms");
}