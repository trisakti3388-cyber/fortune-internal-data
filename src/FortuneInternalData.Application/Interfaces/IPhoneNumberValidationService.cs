namespace FortuneInternalData.Application.Interfaces;

public interface IPhoneNumberValidationService
{
    string Normalize(string? input);
    bool IsValid(string? input);
}
