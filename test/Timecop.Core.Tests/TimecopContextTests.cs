using FluentAssertions;
using TCop.Core.Context;
using TCop.Core.Time;

namespace TCop.Core.Tests;

public class TimecopContextTests
{
    private static readonly PointInTime Base = PointInTime.FromBclTicks(new DateTime(3000, 1, 1).Ticks);
    private static PointInTime BasePlus(int minutes) => Base.Plus(TimeSpan.FromMinutes(minutes));
    private static TimeSpan Minutes(int minutes) => TimeSpan.FromMinutes(minutes);

    [Fact]
    public void GetUtcNow_NoTimeTravel_ShouldReturnCurrentTime()
    {
        var context = new TimecopContext();

        context.GetUtcNow(Base).Should().Be(Base);
    }

    [Fact]
    public void GetUtcNow_Travel10MinutesForward_AndWait5Minutes_ShouldBe15MinutesAhead()
    {
        var context = new TimecopContext();

        context.TravelBy(Minutes(5));

        context.GetUtcNow(BasePlus(5)).Should().Be(BasePlus(10));
    }

    [Fact]
    public void GetUtcNow_FrozenWithoutTimeTravel_After5MinutesHavePassed_ShouldReturnFrozenTime()
    {
        var context = new TimecopContext();

        context.Freeze(Base);

        context.GetUtcNow(BasePlus(5)).Should().Be(Base);
    }

    [Fact]
    public void GetUtcNow_FrozenWith10MinutesTimeTravel_After5MinutesHavePassed_ShouldReturnFrozenTimePlus10Minutes()
    {
        var context = new TimecopContext();

        context.Freeze(Base);
        context.TravelBy(Minutes(10));

        context.GetUtcNow(BasePlus(5)).Should().Be(BasePlus(10));
    }


    [Fact]
    public void GetUtcNow_UnfreezeAfterTravel_ShouldAdjustRelativeTimeCorrectly()
    {
        var context = new TimecopContext();

        context.Freeze(Base);

        context.TravelBy(Minutes(10));

        context.Unfreeze(BasePlus(20));

        context.GetUtcNow(BasePlus(25)).Should().Be(BasePlus(15));
    }

    [Fact]
    public void GetUtcNow_UnfreezeAndTravel_ShouldReturnCorrectTime()
    {
        var context = new TimecopContext();

        context.Freeze(Base);

        context.Unfreeze(BasePlus(20));

        context.TravelBy(Minutes(-15));

        context.GetUtcNow(BasePlus(35)).Should().Be(Base);
    }
}