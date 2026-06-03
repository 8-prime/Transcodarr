namespace Transcodarr.Core.Common.Extensions;

public static class DateTimeExtensions
{
    public static DateTimeOffset ToTruncatedUtcOffset(this DateTime dt)
    {
        var utc = dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc), // Unspecified treated as UTC
        };

        var truncated = new DateTime(
            utc.Year,
            utc.Month,
            utc.Day,
            utc.Hour,
            utc.Minute,
            utc.Second,
            DateTimeKind.Utc
        );

        return new DateTimeOffset(truncated, TimeSpan.Zero);
    }
}
