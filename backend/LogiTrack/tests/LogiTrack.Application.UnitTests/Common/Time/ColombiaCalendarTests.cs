using LogiTrack.Application.Common.Time;

namespace LogiTrack.Application.UnitTests.Common.Time;

public sealed class ColombiaCalendarTests
{
    [Fact]
    public void TodayIn_LateEveningInColombia_IsStillTheSameDay()
    {
        // 2026-11-13 04:30 UTC es 2026-11-12 23:30 en Bogotá.
        var clock = new FixedClock(new DateTimeOffset(2026, 11, 13, 4, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 11, 12), ColombiaCalendar.TodayIn(clock));
    }

    [Fact]
    public void TodayIn_MidDay_IsTheSameDayAsUtc()
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 11, 13, 17, 0, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 11, 13), ColombiaCalendar.TodayIn(clock));
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
