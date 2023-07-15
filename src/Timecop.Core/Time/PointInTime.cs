using System;

namespace TCop.Core.Time;

public readonly struct PointInTime
{
    private static readonly long BclEpochRelativeToUnixEpochTicks = -(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks);

    public long UnixEpochTicks { get; }

    public DateTime DateTimeUtc => new(UnixEpochTicks - BclEpochRelativeToUnixEpochTicks, DateTimeKind.Utc);

    public static PointInTime Now => FromBclTicks(DateTime.UtcNow.Ticks);

    public PointInTime(long unixEpochTicks)
    {
        UnixEpochTicks = unixEpochTicks;
    }

    public static PointInTime FromBclTicks(long bclTicks)
    {
        return new(bclTicks + BclEpochRelativeToUnixEpochTicks);
    }

    public PointInTime Plus(TimeSpan span)
    {
        return new PointInTime(UnixEpochTicks + span.Ticks);
    }

    public PointInTime Minus(TimeSpan span)
    {
        return new PointInTime(UnixEpochTicks - span.Ticks);
    }

    public TimeSpan Minus(PointInTime pointInTime)
    {
        return TimeSpan.FromTicks(UnixEpochTicks - pointInTime.UnixEpochTicks);
    }
}
