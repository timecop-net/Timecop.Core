using FluentAssertions;
using TCop.Core.Context;

namespace TCop.Core.Tests;

public class TimecopContextStoreTests
{
    private static readonly TimeSpan DateTimeComparisonPrecision = TimeSpan.FromMilliseconds(50);

    [Fact]
    public void AsyncContextNow_NoContextStoreInstanceCreated_ShouldReturnCurrentTime()
    {
        TimecopContextStore.AsyncContextNow.DateTimeUtc.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
    }

    [Fact]
    public void AsyncContextNow_ContextStoreInstanceCreated_ShouldReturnCurrentTime()
    {
        var _ = new TimecopContextStore();
        TimecopContextStore.AsyncContextNow.DateTimeUtc.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
    }

    [Fact]
    public void InstanceContextNow_ByDefault_ShouldReturnCurrentTime()
    {
        var store = new TimecopContextStore();
        store.InstanceContextNow.DateTimeUtc.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
    }

    [Fact]
    public async Task AsyncContextNow_TwoNestedStores_ChildInheritsContext_ButParentDoesntSeeChildChanges()
    {
        var store1 = new TimecopContextStore();

        var baseFrozen = store1.Mutate((ref TimecopContext o, PointInTime now) => o.Freeze(now));
        
        store1.Mutate((ref TimecopContext o, PointInTime realNow) => o.TravelBy(TimeSpan.FromMinutes(10), realNow));

        await Task.Run(async () =>
        {
            var store2 = new TimecopContextStore();

            store2.Mutate((ref TimecopContext o, PointInTime realNow) => o.TravelBy(TimeSpan.FromMinutes(20), realNow));
            store2.Mutate((ref TimecopContext o, PointInTime now) => o.Resume(now));

            await Task.Delay(500);

            TimecopContextStore.AsyncContextNow.DateTimeUtc.Should().BeCloseTo(
                baseFrozen.DateTimeUtc
                    .AddMinutes(30)
                    .AddMilliseconds(500),
                DateTimeComparisonPrecision);
        });

        TimecopContextStore.AsyncContextNow.DateTimeUtc.Should().BeCloseTo(
            baseFrozen.DateTimeUtc
                .AddMinutes(10),
            DateTimeComparisonPrecision);
    }

    [Fact]
    public void InstanceContextNow_OwnedByTwoSeparateStores_ShouldActIndependently()
    {
        var store1 = new TimecopContextStore();
        var store2 = new TimecopContextStore();

        var freeze1 = DateTime.UtcNow.AddDays(1);
        var freeze2 = DateTime.UtcNow.AddDays(2);

        store1.Mutate((ref TimecopContext context, PointInTime now) =>
            context.FreezeAt(PointInTime.FromBclTicks(freeze1.Ticks), now));

        store2.Mutate((ref TimecopContext context, PointInTime now) =>
            context.FreezeAt(PointInTime.FromBclTicks(freeze2.Ticks), now));

        store1.InstanceContextNow.DateTimeUtc.Should().Be(freeze1);
        store2.InstanceContextNow.DateTimeUtc.Should().Be(freeze2);
    }


    [Fact]
    public void AsyncContextNow_TwoParallelStores_RunInTwoThreadsInParallel_DoNotInterfereWithEachOther()
    {
        // 0 sec: T1 goes forward 10 minutes, asserts time, waits 2 secs
        // 1 sec: T2 asserts time, goes forward 10 minutes
        // 2 sec: T1 asserts time is still 10 minutes forward

        var task1 = Task.Run(() =>
        {
            var store = new TimecopContextStore();

            store.Mutate((ref TimecopContext o, PointInTime realNow) => o.TravelBy(TimeSpan.FromMinutes(10), realNow));

            TimecopContextStore.AsyncContextNow.DateTimeUtc.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(10), DateTimeComparisonPrecision);

            Thread.Sleep(2000);

            TimecopContextStore.AsyncContextNow.DateTimeUtc.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(10), DateTimeComparisonPrecision);
        });

        var task2 = Task.Run(() =>
        {
            var store = new TimecopContextStore();

            Thread.Sleep(2000);

            TimecopContextStore.AsyncContextNow.DateTimeUtc.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);

            store.Mutate((ref TimecopContext o, PointInTime realNow) => o.TravelBy(TimeSpan.FromMinutes(10), realNow));
        });

        Task.WaitAll(task1, task2);

        TimecopContextStore.AsyncContextNow.DateTimeUtc.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
    }
}