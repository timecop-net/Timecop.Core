using System;

namespace TCop.Core.Context;

public struct TimecopContext
{
    private PointInTime? _subjectiveNow;
    private PointInTime? _lastKnownRealNow;
    private bool _isFrozen;

    public TimecopContext()
    {
        _subjectiveNow = null;
        _lastKnownRealNow = null;
        _isFrozen = false;
    }

    public void TravelBy(TimeSpan duration, PointInTime realNow)
    {
        var now = _subjectiveNow ?? realNow;

        _subjectiveNow = now.Plus(duration);
        _lastKnownRealNow = realNow;
    }

    public void TravelTo(PointInTime destination, PointInTime realNow)
    {
        _subjectiveNow = destination;
        _lastKnownRealNow = realNow;
    }

    public PointInTime GetNow(PointInTime realNow)
    {
        if (_lastKnownRealNow == null || _subjectiveNow == null)
        {
            return realNow;
        }

        if (_isFrozen)
        {
            return _subjectiveNow.Value;
        }

        var lastKnownDelta = _subjectiveNow.Value.Minus(_lastKnownRealNow.Value);

        return realNow.Plus(lastKnownDelta);

    }

    public PointInTime FreezeAt(PointInTime freezeAt, PointInTime realNow)
    {
        _subjectiveNow = freezeAt;
        _lastKnownRealNow = realNow;
        _isFrozen = true;

        return GetNow(realNow);
    }

    public PointInTime Freeze(PointInTime realNow)
    {
        _subjectiveNow ??= realNow;
        _lastKnownRealNow = realNow;
        _isFrozen = true;

        return GetNow(realNow);
    }

    public void Resume(PointInTime realNow)
    {
        _lastKnownRealNow = realNow;
        _isFrozen = false;
    }

    public void Reset()
    {
        _subjectiveNow = null;
        _lastKnownRealNow = null;
        _isFrozen = false;
    }
}


