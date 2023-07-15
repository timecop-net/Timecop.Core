using System;

namespace TCop.Core.Time;

public struct LocalOrUtcDateTime
{
    public readonly DateTime UtcValue;

    public LocalOrUtcDateTime(DateTime value)
    {
        if (value.Kind == DateTimeKind.Unspecified)
        {
            throw new InvalidDateTimeKindException();
        }

        UtcValue = value.ToUniversalTime();
    }

    public PointInTime PointInTime => PointInTime.FromBclTicks(UtcValue.Ticks);
}