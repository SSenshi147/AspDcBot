using DonDumbledore.Logic.Requests;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DonDumbledore.Logic.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterCommands(this IServiceCollection services)
    {
        var types = Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsInterface && !x.IsAbstract && typeof(IDonCommand).IsAssignableFrom(x)).ToList();

        foreach (var type in types)
        {
            services.AddSingleton(typeof(IDonCommand), type);
        }

        return services;
    }
}