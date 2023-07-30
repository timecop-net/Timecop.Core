using System.Threading;

namespace TCop.Core.Context;

public delegate void MutateContextWithCurrentPointInTime(ref TimecopContext context, PointInTime utcNow);
public delegate void MutateContext(ref TimecopContext context);

public class TimecopContextStore
{
    private static readonly AsyncLocal<TimecopContext?> AsyncContext = new();
    private TimecopContext? _instanceContext;

    public PointInTime Mutate(MutateContextWithCurrentPointInTime mutate)
    {
        var asyncContext = AsyncContext.Value ?? new TimecopContext();
        var instanceContext = _instanceContext ?? new TimecopContext();

        var realNow = PointInTime.Now;

        mutate(ref asyncContext, realNow);
        mutate(ref instanceContext, realNow);

        AsyncContext.Value = asyncContext;
        _instanceContext = instanceContext;

        return asyncContext.GetNow(realNow);
    }

    public PointInTime Mutate(MutateContext mutate)
    {
        return Mutate((ref TimecopContext context, PointInTime _) => mutate(ref context));
    }

    public static PointInTime AsyncContextNow => AsyncContext.Value?.GetNow(PointInTime.Now) ?? PointInTime.Now;
    public PointInTime InstanceContextNow => _instanceContext?.GetNow(PointInTime.Now) ?? PointInTime.Now;

    public void ResetContext()
    {
        AsyncContext.Value = null;
    }
}