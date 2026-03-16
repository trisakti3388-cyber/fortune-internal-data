namespace FortuneInternalData.Web.Extensions;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo JakartaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");

    /// <summary>Converts a UTC DateTime to Jakarta time (UTC+7).</summary>
    public static DateTime ToJakartaTime(this DateTime utcDateTime)
    {
        var kind = utcDateTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc)
            : utcDateTime;
        return TimeZoneInfo.ConvertTimeFromUtc(
            kind.Kind == DateTimeKind.Utc ? kind : kind.ToUniversalTime(),
            JakartaTimeZone);
    }

    /// <summary>Converts a nullable UTC DateTime to Jakarta time (UTC+7).</summary>
    public static DateTime? ToJakartaTime(this DateTime? utcDateTime)
        => utcDateTime.HasValue ? utcDateTime.Value.ToJakartaTime() : null;

    /// <summary>Formats a UTC DateTime as "dd/MM/yyyy HH:mm:ss" in Jakarta timezone.</summary>
    public static string ToJakartaDisplay(this DateTime utcDateTime)
        => utcDateTime.ToJakartaTime().ToString("dd/MM/yyyy HH:mm:ss");

    /// <summary>Formats a nullable UTC DateTime as "dd/MM/yyyy HH:mm:ss" in Jakarta timezone.</summary>
    public static string ToJakartaDisplay(this DateTime? utcDateTime)
        => utcDateTime.HasValue ? utcDateTime.Value.ToJakartaDisplay() : string.Empty;
}
