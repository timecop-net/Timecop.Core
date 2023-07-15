using FluentAssertions;

namespace TCop.Core.Tests.TimecopBase;

public class TimecopBaseScopeTests
{
    private static readonly TimeSpan DateTimeComparisonPrecision = TimeSpan.FromMilliseconds(50);

    [Fact]
    public void UtcNow_NoTimecopCreated_ShouldReturnCurrentTime()
    {
        TestTimecopImplementation.UtcNow.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
    }

    [Fact]
    public void UtcNow_TimecopWithoutTravel_ShouldReturnCurrentTime()
    {
        using var tc = new TestTimecopImplementation();
        TestTimecopImplementation.UtcNow.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
    }

    [Fact]
    public void UtcNow_TimecopWithTravelForward_ShouldReturnFutureTimeInsideTimecop_AndCurrentTimeOutsideTimecop()
    {
        using (var tc = new TestTimecopImplementation())
        {
            tc.TravelBy(TimeSpan.FromMinutes(10));

            TestTimecopImplementation.UtcNow.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(10), DateTimeComparisonPrecision);
        }

        TestTimecopImplementation.UtcNow.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
    }

    [Fact]
    public async Task UtcNow_TimecopWithTravelForward_WithAsyncMethod_ShouldShowTheSameTimeBeforeAndAfterAsyncMethod()
    {
        using (var tc = new TestTimecopImplementation())
        {
            tc.TravelBy(TimeSpan.FromMinutes(10));

            await Task.Delay(1000).ConfigureAwait(false);

            TestTimecopImplementation.UtcNow.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(10), DateTimeComparisonPrecision);
        }

        TestTimecopImplementation.UtcNow.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
    }


    [Fact]
    public async Task UtcNow_TwoNestedTimecops_ChildInheritsContext_ButParentDoesntSeeChildChanges()
    {
        using (var tc1 = new TestTimecopImplementation())
        {
            tc1.Freeze();
            var baseFrozen = DateTime.UtcNow;

            tc1.TravelBy(TimeSpan.FromMinutes(10));
            await Task.Run(async () =>
            {
                using (var tc2 = new TestTimecopImplementation())
                {
                    tc2.TravelBy(TimeSpan.FromMinutes(20));

                    tc2.Resume();

                    await Task.Delay(500);

                    TestTimecopImplementation.UtcNow.Should().BeCloseTo(baseFrozen.AddMinutes(30).AddMilliseconds(500),
                        DateTimeComparisonPrecision);
                }
            });

            TestTimecopImplementation.UtcNow.Should().BeCloseTo(baseFrozen.AddMinutes(10), DateTimeComparisonPrecision);
        }
    }


    [Fact]
    public void UtcNow_TwoParallelTimecops_RunInTwoThreadsInParallel_DoNotInterfereWithEachOther()
    {
        // 0 sec: T1 goes forward 10 minutes, asserts time, waits 2 secs
        // 1 sec: T2 asserts time, goes forward 10 minutes
        // 2 sec: T1 asserts time is still 10 minutes forward

        var task1 = Task.Run(() =>
        {
            using var tc = new TestTimecopImplementation();

            tc.TravelBy(TimeSpan.FromMinutes(10));

            TestTimecopImplementation.UtcNow.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(10), DateTimeComparisonPrecision);

            Thread.Sleep(2000);

            TestTimecopImplementation.UtcNow.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(10), DateTimeComparisonPrecision);
        });

        var task2 = Task.Run(() =>
        {
            using var tm = new TestTimecopImplementation();

            Thread.Sleep(2000);

            TestTimecopImplementation.UtcNow.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);

            tm.TravelBy(TimeSpan.FromMinutes(10));
        });

        Task.WaitAll(task1, task2);

        TestTimecopImplementation.UtcNow.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
    }

    [Fact]
    public async Task Freeze_TimeShouldNotRun_AndShouldUnfreezeOnDispose()
    {
        using (var tc = new TestTimecopImplementation())
        {
            tc.Freeze();

            tc.TravelBy(TimeSpan.FromMinutes(10));

            var timeBeforeDelay = TestTimecopImplementation.UtcNow;

            await Task.Delay(1000);

            TestTimecopImplementation.UtcNow.Should().Be(timeBeforeDelay);
        }

        TestTimecopImplementation.UtcNow.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
    }
}