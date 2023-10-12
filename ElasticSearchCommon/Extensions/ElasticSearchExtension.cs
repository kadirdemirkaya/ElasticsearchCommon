using ElasticSearchCommon.Repositories;
using ElasticSearchCommon.Services;
using ElasticSearchCommon.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace ElasticSearchCommon.Extensions;

public static class ElasticsearchExtensions
{
    public static IServiceCollection AddElasticsearch(this IServiceCollection services, ElasticConfiguration elasticConfiguration)
    {
        var defaultIndex = elasticConfiguration.DefaultIndex;
        var basicAuthUser = elasticConfiguration.UserName;
        var basicAuthPassword = elasticConfiguration.Password;

        var settings = new ConnectionSettings(new Uri(elasticConfiguration.ElasticUrl));

        if (!string.IsNullOrEmpty(defaultIndex))
            settings = settings.DefaultIndex(defaultIndex);

        if (!string.IsNullOrEmpty(basicAuthUser) && !string.IsNullOrEmpty(basicAuthPassword))
            settings = settings.BasicAuthentication(basicAuthUser, basicAuthPassword);

        settings.EnableApiVersioningHeader();

        var client = new ElasticClient(settings);

        services.AddSingleton<IElasticClient>(client);

        services.AddScoped(typeof(IElasticSearchRepository<>), typeof(ElasticRepository<>));

        services.AddScoped(typeof(IElasticsearchService<>), typeof(ElasticsearchService<>));

        return services;
    }

    public static IApplicationBuilder UseElasticApp(this IApplicationBuilder app, IConfiguration configuration)
    {
        //app.UseAllElasticApm(configuration);
        return app;
    }
}