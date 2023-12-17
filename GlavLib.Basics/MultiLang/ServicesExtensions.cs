using Microsoft.Extensions.DependencyInjection;

namespace GlavLib.Basics.MultiLang;

public static class ServicesExtensions
{
    public static IServiceCollection AddMultiLang(this IServiceCollection services, Action<LanguageContextBuilder> setup)
    {
        var builder = new LanguageContextBuilder();

        setup(builder);


        var languageContext = builder.Build();

        services.AddSingleton(languageContext);

        return services;
    }
}