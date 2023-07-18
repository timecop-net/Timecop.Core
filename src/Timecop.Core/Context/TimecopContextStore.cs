using System.Threading;
using TCop.Core.Time;

namespace TCop.Core.Context;

public delegate void MutateContextWithCurrentPointInTime(ref TimecopContext context, PointInTime utcNow);
public delegate void MutateContext(ref TimecopContext context);

public class TimecopContextStore
{
    private static readonly AsyncLocal<TimecopContext?> AsyncContext = new();

    public PointInTime Mutate(MutateContextWithCurrentPointInTime mutate)
    {
        var asyncContext = AsyncContext.Value ?? new TimecopContext();

        var realNow = PointInTime.Now;

        mutate(ref asyncContext, realNow);

        AsyncContext.Value = asyncContext;

        return asyncContext.GetNow(realNow);
    }

    public PointInTime Mutate(MutateContext mutate)
    {
        return Mutate((ref TimecopContext context, PointInTime _) => mutate(ref context));
    }

    public static PointInTime AsyncContextUtcNow => AsyncContext.Value?.GetNow(PointInTime.Now) ?? PointInTime.Now;

    public void ResetContext()
    {
        AsyncContext.Value = null;
    }
}