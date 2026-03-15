using System.Text.RegularExpressions;
using FortuneInternalData.Application.Interfaces;

namespace FortuneInternalData.Application.Validators;

public class PhoneNumberValidator : IPhoneNumberValidationService
{
    private static readonly Regex DigitsOnlyRegex = new("^[0-9]{10,14}$", RegexOptions.Compiled);

    public string Normalize(string? input)
    {
        return (input ?? string.Empty).Trim();
    }

    public bool IsValid(string? input)
    {
        var normalized = Normalize(input);
        return DigitsOnlyRegex.IsMatch(normalized);
    }
}
