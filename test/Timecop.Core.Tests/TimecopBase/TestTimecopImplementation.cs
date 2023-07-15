using Timecop.Core;

namespace TCop.Core.Tests.TimecopBase;

public class TestTimecopImplementation: TimecopBase<TestTimecopImplementation>
{
    public static DateTime UtcNow => UtcNowPointInTime.DateTimeUtc;
}