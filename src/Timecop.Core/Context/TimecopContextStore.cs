using System.Threading;
using TCop.Core.Time;

namespace TCop.Core.Context;

public class TimecopContextStore
{
    private static readonly AsyncLocal<TimecopContext?> AsyncContext = new();

    public delegate void MutateContextWithCurrentDateTime(ref TimecopContext context, PointInTime utcNow);
    public delegate void MutateContext(ref TimecopContext context);

    public void Mutate(MutateContextWithCurrentDateTime mutate)
    {
        var asyncContext = AsyncContext.Value ?? new TimecopContext();

        mutate(ref asyncContext, PointInTime.Now);

        AsyncContext.Value = asyncContext;
    }

    public void Mutate(MutateContext mutate)
    {
        Mutate((ref TimecopContext context, PointInTime _) => mutate(ref context));
    }

    public static PointInTime AsyncContextUtcNow => AsyncContext.Value?.GetUtcNow(PointInTime.Now) ?? PointInTime.Now;

    public void ResetContext()
    {
        AsyncContext.Value = null;
    }
}