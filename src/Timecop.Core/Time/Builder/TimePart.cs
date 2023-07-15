namespace TCop.Core.Time.Builder;

public class TimePart
{
    public int Hour { get; }
    public int Minute { get; }
    public int Second { get; }
    public int Millisecond { get; }

    public TimePart(int hour, int minute, int second, int millisecond)
    {
        Hour = hour;
        Minute = minute;
        Second = second;
        Millisecond = millisecond;
    }
}