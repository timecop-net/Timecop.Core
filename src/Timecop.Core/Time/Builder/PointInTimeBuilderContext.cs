using System;

namespace TCop.Core.Time.Builder;

internal class PointInTimeBuilderContext
{
    public BaseTimePoint BaseTimePoint { get; set; } = BaseTimePoint.Current;

    public DatePart? Date { get; set; }

    public TimePart? Time { get; set; }

    public DateTimeKind Kind { get; set; } = DateTimeKind.Unspecified;
}