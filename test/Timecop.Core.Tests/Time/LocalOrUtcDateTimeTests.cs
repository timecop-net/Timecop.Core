using FluentAssertions;
using TCop.Core.Time;

namespace TCop.Core.Tests.Time;

public class LocalOrUtcDateTimeTests
{
    [Fact]
    public void UtcValue_GivenUtcDateTime_ShouldNotConvert()
    {
        var utcValue = new DateTime(1990, 12, 2, 0, 0, 0, DateTimeKind.Utc);

        var utcDateTime = new LocalOrUtcDateTime(utcValue);

        utcDateTime.UtcValue.Should().Be(utcValue);
    }

    [Fact]
    public void UtcValue_GivenLocalDateTime_ShouldConvertToUtc()
    {
        TimeZoneInfo.Local.BaseUtcOffset.Should().NotBe(TimeSpan.Zero);

        var utcValue = new DateTime(1990, 12, 2, 0, 0, 0, DateTimeKind.Utc);
        var localValue = utcValue.ToLocalTime();

        var utcDateTime = new LocalOrUtcDateTime(localValue);

        utcDateTime.UtcValue.Should().Be(utcValue);
    }

    [Fact]
    public void UtcValue_GivenUnspecifiedDateTime_ShouldThrowAnException()
    {
        var unspecifiedDateTime = new DateTime(1990, 12, 2, 0, 0, 0, DateTimeKind.Unspecified);

        var createUtcDateTime = () => new LocalOrUtcDateTime(unspecifiedDateTime);

        createUtcDateTime.Should().Throw<InvalidDateTimeKindException>()
            .WithMessage("DateTimeKind.Unspecified is not supported. Use DateTimeKind.Utc or DateTimeKind.Local.");
    }
}