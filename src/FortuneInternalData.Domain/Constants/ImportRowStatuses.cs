namespace FortuneInternalData.Domain.Constants;

public static class ImportRowStatuses
{
    public const string New = "new";
    public const string Existing = "existing";
    public const string Invalid = "invalid";
    public const string DuplicateFile = "duplicate_file";
    public const string Matched = "matched";      // update/web_status: found in DB
    public const string Found = "found";          // delete: found in DB
    public const string NotFound = "not_found";   // update/delete: not in DB
}
