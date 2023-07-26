using FluentAssertions;
using TCop.Core.Context;

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

        context.GetNow(Base).Should().Be(Base);
    }

    [Fact]
    public void GetUtcNow_Travel10MinutesForward_AndWait5Minutes_ShouldBe15MinutesAhead()
    {
        var context = new TimecopContext();

        context.TravelBy(Minutes(5), Base);

        context.GetNow(BasePlus(5)).Should().Be(BasePlus(10));
    }

    [Fact]
    public void GetUtcNow_Travel10MinutesTwice_ShouldBe20MinutesAhead()
    {
        var context = new TimecopContext();

        context.TravelBy(Minutes(10), Base);
        context.TravelBy(Minutes(10), Base);

        context.GetNow(Base).Should().Be(BasePlus(20));
    }

    [Fact]
    public void GetUtcNow_TravelTo_AfterTravelBy_ShouldReturnTravelToTime()
    {
        var context = new TimecopContext();

        var travelTo = BasePlus(30);

        context.TravelBy(Minutes(10), Base);

        context.TravelTo(travelTo, Base);
        
        context.GetNow(Base).Should().Be(travelTo);
    }

    [Fact]
    public void GetUtcNow_Resume_AfterFreezeAt_ShouldReturnFreezedAtTimePlusElapsedTime()
    {
        var context = new TimecopContext();

        var freezeAt = BasePlus(30);

        context.FreezeAt(freezeAt, Base);
        context.Resume(Base);

        context.GetNow(BasePlus(5)).Should().Be(freezeAt.Plus(TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void GetUtcNow_Freeze_AfterTravelTo_ShouldReturnTimeItWasFrozenAt()
    {
        var context = new TimecopContext();

        var travelTo = BasePlus(30);

        context.TravelTo(travelTo, Base);

        context.Freeze(Base);

        context.GetNow(Base).Should().Be(travelTo);
    }

    [Fact]
    public void GetUtcNow_FrozenWithoutTimeTravel_After5MinutesHavePassed_ShouldReturnFrozenTime()
    {
        var context = new TimecopContext();

        context.Freeze(Base);

        context.GetNow(BasePlus(5)).Should().Be(Base);
    }

    [Fact]
    public void GetUtcNow_FrozenWith10MinutesTimeTravel_After5MinutesHavePassed_ShouldReturnFrozenTimePlus10Minutes()
    {
        var context = new TimecopContext();

        context.Freeze(Base);
        context.TravelBy(Minutes(10), Base);

        context.GetNow(BasePlus(5)).Should().Be(BasePlus(10));
    }


    [Fact]
    public void GetUtcNow_UnfreezeAfterTravel_ShouldAdjustRelativeTimeCorrectly()
    {
        var context = new TimecopContext();

        context.Freeze(Base); // real: 0, timecop: 0

        context.TravelBy(Minutes(10), Base); // real: 0, timecop: +10

        context.Resume(BasePlus(20)); // real: 20, timecop: +10

        context.GetNow(BasePlus(25)).Should().Be(BasePlus(15)); // real: 25, timecop: +15
    }

    [Fact]
    public void GetUtcNow_UnfreezeAndTravel_ShouldReturnCorrectTime()
    {
        var context = new TimecopContext();

        context.Freeze(Base);

        context.Resume(BasePlus(20));

        context.TravelBy(Minutes(-15), BasePlus(20));

        context.GetNow(BasePlus(35)).Should().Be(Base);
    }

    [Fact]
    public void GetUtcNow_Reset_AfterTravel_ShouldReturnCurrentTime()
    {
        var context = new TimecopContext();

        context.Freeze(Base);
        context.TravelBy(TimeSpan.FromMinutes(10), Base);

        context.Reset();

        context.GetNow(BasePlus(25)).Should().Be(BasePlus(25));
    }
}