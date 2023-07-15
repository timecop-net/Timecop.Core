using FluentAssertions;
using TCop.Core.Time;

namespace TCop.Core.Tests.Time;

public class PointInTimeTests
{
    [Fact]
    public void Ctor_GivenDateTime_ShouldReturnPointInTimeRepresentingTheDateTime()
    {
        var dateTime = DateTime.UtcNow;

        var pit = PointInTime.FromBclTicks(dateTime.Ticks);

        pit.DateTimeUtc.Should().Be(dateTime);
    }
}