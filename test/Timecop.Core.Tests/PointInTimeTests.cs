using FluentAssertions;

namespace TCop.Core.Tests;

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