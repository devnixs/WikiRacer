using WikiRacer.Application.Abstractions.Clock;

namespace WikiRacer.Infrastructure.Clock;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
