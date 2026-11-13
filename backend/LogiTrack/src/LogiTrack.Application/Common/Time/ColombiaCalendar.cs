namespace LogiTrack.Application.Common.Time;

// "Hoy" para la operación de la empresa es la fecha en Colombia (America/Bogota, UTC-5, sin horario de verano).
// Usar la fecha UTC cambiaría el día entre las 19:00 y las 24:00 hora local.
public static class ColombiaCalendar
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");

    public static DateOnly TodayIn(TimeProvider timeProvider)
    {
        var local = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), Zone);
        return DateOnly.FromDateTime(local.DateTime);
    }
}
