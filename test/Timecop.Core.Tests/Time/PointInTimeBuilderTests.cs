using FluentAssertions;
using TCop.Core.Time.Builder;

namespace TCop.Core.Tests.Time;

public class PointInTimeBuilderTests
{
    private static readonly TimeSpan DateTimeComparisonPrecision = TimeSpan.FromMilliseconds(50);

    private readonly PointInTimeBuilder _builder = new();

    [Fact]
    public void LocalTime_ShouldReturnCurrentLocalTime()
    {
        _builder.LocalTime();

        _builder.Build().DateTimeUtc.ToLocalTime().Should().BeCloseTo(DateTime.Now, DateTimeComparisonPrecision);
    }

    [Fact]
    public void UtcTime_ShouldReturnLocalTime()
    {
        _builder.UtcTime();

        _builder.Build().DateTimeUtc.Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
    }

    [Fact]
    public void Build_OnWasCalled_ButNeitherLocalNorUtcWasCalled_ShouldThrow()
    {
        _builder.On(1990, 12, 2);

        var build = () => _builder.Build();

        build.Should().Throw<PointInTimeBuilderNeitherLocalNorUtcException>().WithMessage("Call either LocalTime() or UtcTime() when configuring the point in time.");
    }

    [Fact]
    public void Build_AtWasCalled_ButNeitherLocalNorUtcWasCalled_ShouldThrow()
    {
        _builder.At(14, 0, 0);

        var build = () => _builder.Build();

        build.Should().Throw<PointInTimeBuilderNeitherLocalNorUtcException>().WithMessage("Call either LocalTime() or UtcTime() when configuring the point in time.");
    }

    [Fact]
    public void InTheFuture_ShouldReturnPointOfTimeInTheFuture()
    {
        _builder.InTheFuture();

        _builder.Build().DateTimeUtc.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void InThePast_ShouldReturnPointOfTimeInThePast()
    {
        _builder.InThePast();

        _builder.Build().DateTimeUtc.Should().BeBefore(DateTime.UtcNow);
    }

    [Fact]
    public void On_ShouldReturnSetDateAndCurrentTime()
    {
        _builder
            .On(1990, 12, 2)
            .LocalTime();

        var now = DateTime.Now;

        _builder.Build().DateTimeUtc.ToLocalTime().Should().BeCloseTo(new DateTime(1990, 12, 2,
            now.Hour, now.Minute, now.Second, now.Millisecond, DateTimeKind.Local), DateTimeComparisonPrecision);
    }

    [Fact]
    public void At_ShouldReturnSetTimeAndCurrentDate()
    {
        _builder
            .At(14, 15, 30, 893)
            .LocalTime();

        var now = DateTime.Now;

        _builder.Build().DateTimeUtc.ToLocalTime().Should().BeCloseTo(new DateTime(now.Year, now.Month, now.Day,
            14, 15, 30, 893, DateTimeKind.Local), DateTimeComparisonPrecision);
    }
}