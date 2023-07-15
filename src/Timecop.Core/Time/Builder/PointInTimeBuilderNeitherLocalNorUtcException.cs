using System;

namespace TCop.Core.Time.Builder;

public class PointInTimeBuilderNeitherLocalNorUtcException : Exception
{
    public PointInTimeBuilderNeitherLocalNorUtcException() : base($"Call either {nameof(PointInTimeBuilder.LocalTime)}() or {nameof(PointInTimeBuilder.UtcTime)}() when configuring the point in time.")
    {
    }
}