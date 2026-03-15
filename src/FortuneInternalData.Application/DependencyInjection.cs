using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FortuneInternalData.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPhoneNumberService, PhoneNumberService>();
        return services;
    }
}
