using System;

namespace TCop.Core.Time.Builder;

public class PointInTimeBuilder
{
    private readonly PointInTimeBuilderContext _context = new ();

    public PointInTimeBuilder At(int hour, int minute, int second, int millisecond = 0)
    {
        _context.Time = new TimePart(hour, minute, second, millisecond);
        return this;
    }

    public PointInTimeBuilder On(int year, int month, int day)
    {
        _context.Date = new DatePart(year, month, day);
        return this;
    }

    public PointInTimeBuilder LocalTime()
    {
        _context.Kind = DateTimeKind.Local;
        return this;
    }

    public PointInTimeBuilder UtcTime()
    {
        _context.Kind = DateTimeKind.Utc;
        return this;
    }

    internal PointInTime Build()
    {
        if (LocalOrUtcShouldHaveBeenSpecifiedExplicitly())
        {
            throw new PointInTimeBuilderNeitherLocalNorUtcException();
        }

        var baseDateTime = GetBaseDateTime();

        _context.Date ??= new DatePart(baseDateTime.Year, baseDateTime.Month, baseDateTime.Day);
        _context.Time ??= new TimePart(baseDateTime.Hour, baseDateTime.Minute, baseDateTime.Second, baseDateTime.Millisecond);

        var dateTime = new DateTime(_context.Date.Year, _context.Date.Month, _context.Date.Day,
            _context.Time.Hour, _context.Time.Minute, _context.Time.Second, _context.Time.Millisecond, _context.Kind);

        return PointInTime.FromBclTicks(dateTime.ToUniversalTime().Ticks);
    }

    private DateTime GetBaseDateTime()
    {
        var now = _context.Kind == DateTimeKind.Local ? DateTime.Now : DateTime.UtcNow;

        if (_context.BaseTimePoint == BaseTimePoint.Current)
            return now;

        var randomDouble = new Random().NextDouble();

        var randomOffsetInTicks = (long)((TimeSpan.FromDays(30).Ticks - TimeSpan.FromSeconds(2).Ticks)*(1 - randomDouble));

        if (_context.BaseTimePoint == BaseTimePoint.Past)
        {
            now = now.Subtract(TimeSpan.FromTicks(randomOffsetInTicks));
        }

        if (_context.BaseTimePoint == BaseTimePoint.Future)
        {
            now = now.Add(TimeSpan.FromTicks(randomOffsetInTicks));
        }

        return now;
    }

    private bool LocalOrUtcShouldHaveBeenSpecifiedExplicitly()
    {
        return (_context.Date != null || _context.Time != null) && _context.Kind == DateTimeKind.Unspecified;
    }

    public void InTheFuture()
    {
        _context.BaseTimePoint = BaseTimePoint.Future;
    }

    public void InThePast()
    {
        _context.BaseTimePoint = BaseTimePoint.Past;
    }
}