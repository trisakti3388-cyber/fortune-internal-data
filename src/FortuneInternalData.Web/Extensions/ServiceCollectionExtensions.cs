using FortuneInternalData.Application;
using FortuneInternalData.Infrastructure;
using FortuneInternalData.Web.Security;

namespace FortuneInternalData.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFortuneInternalData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddAppAuthorization();

        return services;
    }
}
