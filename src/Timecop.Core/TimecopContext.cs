using System;

namespace TCop.Core;

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

    public PointInTime GetUtcNow(PointInTime realUtcNow)
    {
        var baseTime = _lastRealTimeFrozenAt ?? realUtcNow;

        var baseTimeWithDelta = baseTime.Plus(_timeTravelDelta);

        return baseTimeWithDelta.Minus(_totalRealTimePassedWhileFrozen);
    }

    public PointInTime Freeze(PointInTime utcNow)
    {
        _lastRealTimeFrozenAt = utcNow;

        return GetUtcNow(utcNow);
    }

    public void Unfreeze(PointInTime utcNow)
    {
        if (!_lastRealTimeFrozenAt.HasValue)
            return;

        var realTimePassedInFrozenState = utcNow.Minus(_lastRealTimeFrozenAt.Value);

        _totalRealTimePassedWhileFrozen = _totalRealTimePassedWhileFrozen.Add(realTimePassedInFrozenState);

        _lastRealTimeFrozenAt = null;
    }
}