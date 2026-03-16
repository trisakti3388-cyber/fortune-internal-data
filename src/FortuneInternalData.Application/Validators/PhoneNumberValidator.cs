using System.Text.RegularExpressions;
using FortuneInternalData.Application.Interfaces;

namespace FortuneInternalData.Application.Validators;

public class PhoneNumberValidator : IPhoneNumberValidationService
{
    private static readonly Regex DigitsOnlyRegex = new("^[0-9]{8,20}$", RegexOptions.Compiled);
    private static readonly Regex StripCharsRegex = new(@"[\+\s\-\.\(\)]", RegexOptions.Compiled);

    public string Normalize(string? input)
    {
        var s = (input ?? string.Empty).Trim();
        return StripCharsRegex.Replace(s, string.Empty);
    }

    public bool IsValid(string? input)
    {
        var normalized = Normalize(input);
        return DigitsOnlyRegex.IsMatch(normalized);
    }
}
