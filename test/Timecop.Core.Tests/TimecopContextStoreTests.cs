using FluentAssertions;
using TCop.Core.Context;

namespace TCop.Core.Tests;

public class TimecopContextStoreTests
{
    private static readonly TimeSpan DateTimeComparisonPrecision = TimeSpan.FromMilliseconds(50);

    [Fact]
    public void AsyncContextUtcNow_NoContextStoreInstanceCreated_ShouldReturnCurrentTime()
    {
        TimecopContextStore.AsyncContextUtcNow.DateTimeUtc.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
    }

    [Fact]
    public void AsyncContextUtcNow_ContextStoreInstanceCreated_ShouldReturnCurrentTime()
    {
        var _ = new TimecopContextStore();
        TimecopContextStore.AsyncContextUtcNow.DateTimeUtc.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
    }

    [Fact]
    public async Task AsyncContextUtcNow_TwoNestedStores_ChildInheritsContext_ButParentDoesntSeeChildChanges()
    {
        var store1 = new TimecopContextStore();

        var baseFrozen = store1.Mutate((ref TimecopContext o, PointInTime now) => o.Freeze(now));
        
        store1.Mutate((ref TimecopContext o) => o.TravelBy(TimeSpan.FromMinutes(10)));

        await Task.Run(async () =>
        {
            var store2 = new TimecopContextStore();

            store2.Mutate((ref TimecopContext o) => o.TravelBy(TimeSpan.FromMinutes(20)));
            store2.Mutate((ref TimecopContext o, PointInTime now) => o.Resume(now));

            await Task.Delay(500);

            TimecopContextStore.AsyncContextUtcNow.DateTimeUtc.Should().BeCloseTo(
                baseFrozen.DateTimeUtc
                    .AddMinutes(30)
                    .AddMilliseconds(500),
                DateTimeComparisonPrecision);
        });

        TimecopContextStore.AsyncContextUtcNow.DateTimeUtc.Should().BeCloseTo(
            baseFrozen.DateTimeUtc
                .AddMinutes(10),
            DateTimeComparisonPrecision);
    }


    [Fact]
    public void AsyncContextUtcNow_TwoParallelStores_RunInTwoThreadsInParallel_DoNotInterfereWithEachOther()
    {
        // 0 sec: T1 goes forward 10 minutes, asserts time, waits 2 secs
        // 1 sec: T2 asserts time, goes forward 10 minutes
        // 2 sec: T1 asserts time is still 10 minutes forward

        var task1 = Task.Run(() =>
        {
            var store = new TimecopContextStore();

            store.Mutate((ref TimecopContext o) => o.TravelBy(TimeSpan.FromMinutes(10)));

            TimecopContextStore.AsyncContextUtcNow.DateTimeUtc.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(10), DateTimeComparisonPrecision);

            Thread.Sleep(2000);

            TimecopContextStore.AsyncContextUtcNow.DateTimeUtc.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(10), DateTimeComparisonPrecision);
        });

        var task2 = Task.Run(() =>
        {
            var store = new TimecopContextStore();

            Thread.Sleep(2000);

            TimecopContextStore.AsyncContextUtcNow.DateTimeUtc.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);

            store.Mutate((ref TimecopContext o) => o.TravelBy(TimeSpan.FromMinutes(10)));
        });

        Task.WaitAll(task1, task2);

        TimecopContextStore.AsyncContextUtcNow.DateTimeUtc.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
    }
}