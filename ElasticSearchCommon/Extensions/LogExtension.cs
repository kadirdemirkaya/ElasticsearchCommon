using Elastic.CommonSchema.Serilog;
using ElasticSearchCommon.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Filters;
using Serilog.Sinks.Elasticsearch;

namespace ElasticSearchCommon.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddLogSeriLog(this WebApplicationBuilder builder, ElasticConfiguration ElasticConfiguration, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
            .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
            .Enrich.FromLogContext()
            .Enrich.WithCorrelationId()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ApplicationName", $"{ElasticConfiguration.ApplicationName}")
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles"))
            .WriteTo.Async(writeTo => writeTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"))
            .WriteTo.Async(writeTo => writeTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(ElasticConfiguration.ElasticUrl))
            {
                TypeName = null,
                AutoRegisterTemplate = true,
                IndexFormat = ElasticConfiguration.DefaultIndex,
                BatchAction = ElasticOpType.Create,
                CustomFormatter = new EcsTextFormatter(),
                ModifyConnectionSettings = x => x.BasicAuthentication(ElasticConfiguration.UserName, ElasticConfiguration.Password)
            }))
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Host.UseSerilog(Log.Logger, true);

        return builder;
    }
}