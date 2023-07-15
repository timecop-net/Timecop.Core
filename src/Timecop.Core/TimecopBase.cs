using System;
using TCop.Core.Context;
using TCop.Core.Time;

namespace Timecop.Core;

public abstract class TimecopBase<TTimecop> : IDisposable where TTimecop: TimecopBase<TTimecop>, new()
{
    private readonly TimecopContextStore _contextStore = new();

    protected static PointInTime UtcNowPointInTime => TimecopContextStore.AsyncContextUtcNow;

    /// <summary>Moves in time backwards or forwards by the specified amount of time.</summary>
    /// <param name="duration">The amount of time to travel by. Can be positive or negative.</param>
    public void TravelBy(TimeSpan duration)
    {
        _contextStore.Mutate((ref TimecopContext context) => context.TravelBy(duration));
    }

    protected void ConvertAndFreeze(LocalOrUtcDateTime? dateTime)
    {
        _contextStore.Mutate((ref TimecopContext context, PointInTime utcNow) => context.Freeze(dateTime?.PointInTime ?? utcNow));
    }

    /// <summary>Freezes an instance of <see cref="T:TCop.Timecop" /> at the current time.</summary>
    public TTimecop Freeze()
    {
        ConvertAndFreeze(null);
        return (TTimecop)this;
    }

    /// <summary>Freezes an instance of <see cref="T:TCop.Timecop" /> at the specified local or UTC time.</summary>
    /// <param name="frozenAt">The time to freeze at. Must represent either local or UTC time.</param>
    /// <exception cref="T:TCop.DateTimeUtils.InvalidDateTimeKindException">
    ///     <paramref name="frozenAt" /> has the kind of Unspecified.
    /// </exception>
    public TTimecop Freeze(DateTime freezeAt)
    {
        ConvertAndFreeze(new LocalOrUtcDateTime(freezeAt));
        return (TTimecop)this;
    }

    /// <summary>Freezes an instance of <see cref="T:TCop.Timecop" /> at the specified local or UTC time.</summary>
    /// <param name="year">The year (1 through 9999).</param>
    /// <param name="month">The month (1 through 12).</param>
    /// <param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
    /// <param name="hour">The hours (0 through 23).</param>
    /// <param name="minute">The minutes (0 through 59).</param>
    /// <param name="second">The seconds (0 through 59).</param>
    /// <param name="kind">One of the enumeration values that indicates whether <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" /> and <paramref name="second" /> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
    /// <exception cref="T:TCop.DateTimeUtils.InvalidDateTimeKindException">
    ///     <paramref name="kind" /> has the kind of Unspecified.
    /// </exception>
    public TTimecop Freeze(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
    {
        return Freeze(new DateTime(year, month, day, hour, minute, second, kind));
    }

    /// <summary>Freezes an instance of <see cref="T:TCop.Timecop" /> at the specified local or UTC time.</summary>
    /// <param name="year">The year (1 through 9999).</param>
    /// <param name="month">The month (1 through 12).</param>
    /// <param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
    /// <param name="kind">One of the enumeration values that indicates whether <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" /> and <paramref name="second" /> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
    /// <exception cref="T:TCop.DateTimeUtils.InvalidDateTimeKindException">
    ///     <paramref name="kind" /> has the kind of Unspecified.
    /// </exception>
    public TTimecop Freeze(int year, int month, int day, DateTimeKind kind)
    {
        return Freeze(new DateTime(year, month, day, 0, 0, 0, kind));
    }
    

    /// <summary>Resumes the flow of time of a frozen instance of <see cref="T:TCop.Timecop" />.</summary>
    public void Resume()
    {
        _contextStore.Mutate((ref TimecopContext context, PointInTime utcNow) => context.Unfreeze(utcNow));
    }

    /// <summary>Creates an instance of <see cref="T:TCop.Timecop" /> and freezes it at the current time.</summary>
    public static TTimecop Frozen()
    {
        return new TTimecop().Freeze();
    }

    /// <summary>Creates an instance of <see cref="T:TCop.Timecop" /> and freezes it at the specified local or UTC time.</summary>
    /// <param name="frozenAt">The time to freeze at. Must represent either local or UTC time.</param>
    /// <exception cref="T:TCop.DateTimeUtils.InvalidDateTimeKindException">
    ///     <paramref name="frozenAt" /> has the kind of Unspecified.
    /// </exception>
    public static TTimecop Frozen(DateTime frozenAt)
    {
        return new TTimecop().Freeze(frozenAt);
    }

    /// <summary>Creates an instance of <see cref="T:TCop.Timecop" /> and freezes it at the specified local or UTC time.</summary>
    /// <param name="year">The year (1 through 9999).</param>
    /// <param name="month">The month (1 through 12).</param>
    /// <param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
    /// <param name="hour">The hours (0 through 23).</param>
    /// <param name="minute">The minutes (0 through 59).</param>
    /// <param name="second">The seconds (0 through 59).</param>
    /// <param name="kind">One of the enumeration values that indicates whether <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" /> and <paramref name="second" /> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
    /// <exception cref="T:TCop.DateTimeUtils.InvalidDateTimeKindException">
    ///     <paramref name="kind" /> has the kind of Unspecified.
    /// </exception>
    public static TTimecop Frozen(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
    {
        return new TTimecop().Freeze(new DateTime(year, month, day, hour, minute, second, kind));
    }

    /// <summary>Creates an instance of <see cref="T:TCop.Timecop" /> and freezes it at the specified local or UTC time.</summary>
    /// <param name="year">The year (1 through 9999).</param>
    /// <param name="month">The month (1 through 12).</param>
    /// <param name="day">The day (1 through the number of days in <paramref name="month" />).</param>
    /// <param name="kind">One of the enumeration values that indicates whether <paramref name="year" />, <paramref name="month" />, <paramref name="day" />, <paramref name="hour" />, <paramref name="minute" /> and <paramref name="second" /> specify a local time, Coordinated Universal Time (UTC), or neither.</param>
    /// <exception cref="T:TCop.DateTimeUtils.InvalidDateTimeKindException">
    ///     <paramref name="kind" /> has the kind of Unspecified.
    /// </exception>
    public static TTimecop Frozen(int year, int month, int day, DateTimeKind kind)
    {
        return new TTimecop().Freeze(new DateTime(year, month, day, 0, 0, 0, kind));
    }
    
    public void Dispose()
    {
        _contextStore.ResetContext();
    }
}