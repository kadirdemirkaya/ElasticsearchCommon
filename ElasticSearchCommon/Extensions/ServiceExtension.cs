using ElasticSearchCommon.Repositories;
using ElasticSearchCommon.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ElasticSearchCommon.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddElasticSearch(this IServiceCollection services, Action<ElasticConfiguration> configure)
    {
        var elasticConfiguration = new ElasticConfiguration();
        configure(elasticConfiguration);

        services.AddElasticsearch(elasticConfiguration);

        return services;
    }

    public static IApplicationBuilder AddElasticSearchApp(this IApplicationBuilder app, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        app.UseElasticApp(configuration);

        return app;
    }

    public static WebApplicationBuilder AddElasticSearchBuilder(this WebApplicationBuilder app, Action<ElasticConfiguration> configure, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        var elasticConfiguration = new ElasticConfiguration();
        configure(elasticConfiguration);

        app.AddLogSeriLog(elasticConfiguration, configuration);

        return app;
    }
}