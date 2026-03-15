namespace FortuneInternalData.Domain.Constants;

public static class WhatsappStatuses
{
    public const string OneDay = "1day";
    public const string ThreeDay = "3day";
    public const string SevenDay = "7day";
    public const string Active = "active";
    public const string Inactive = "inactive";

    public static readonly string[] All =
    {
        OneDay,
        ThreeDay,
        SevenDay,
        Active,
        Inactive
    };
}
