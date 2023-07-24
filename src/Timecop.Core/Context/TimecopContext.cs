using System;

namespace TCop.Core.Context;

public struct TimecopContext
{
    private PointInTime? _lastRealTimeFrozenAt;

    private TimeSpan _timeTravelDelta;

    private TimeSpan _totalRealTimePassedWhileFrozen;

    public TimecopContext()
    {
        _timeTravelDelta = TimeSpan.Zero;
        _totalRealTimePassedWhileFrozen = TimeSpan.Zero;
        _lastRealTimeFrozenAt = null;
    }

    public void TravelBy(TimeSpan duration)
    {
        _timeTravelDelta = _timeTravelDelta.Add(duration);
    }

    public PointInTime GetNow(PointInTime realNow)
    {
        var baseTime = _lastRealTimeFrozenAt ?? realNow;

        var baseTimeWithDelta = baseTime.Plus(_timeTravelDelta);

        return baseTimeWithDelta.Minus(_totalRealTimePassedWhileFrozen);
    }

    public PointInTime Freeze(PointInTime now)
    {
        _lastRealTimeFrozenAt = now;

        return GetNow(now);
    }

    public void Resume(PointInTime now)
    {
        if (!_lastRealTimeFrozenAt.HasValue)
            return;

        var realTimePassedInFrozenState = now.Minus(_lastRealTimeFrozenAt.Value);

        _totalRealTimePassedWhileFrozen = _totalRealTimePassedWhileFrozen.Add(realTimePassedInFrozenState);

        _lastRealTimeFrozenAt = null;
    }
}