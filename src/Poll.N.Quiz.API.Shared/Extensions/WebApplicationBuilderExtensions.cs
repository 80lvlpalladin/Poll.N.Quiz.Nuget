using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Poll.N.Quiz.API.Shared.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddConcurrencyRateLimiter
        (this WebApplicationBuilder builder, int? concurrentRequestsLimit = null)
    {
        if (concurrentRequestsLimit is null)
        {
            var configurationSection = "RateLimiting:ConcurrentRequestsLimit";

            concurrentRequestsLimit =
                builder.Configuration.GetRequiredSection(configurationSection).Get<ushort>();
        }


        builder.Services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
            rateLimiterOptions.AddConcurrencyLimiter("concurrency", concurrencyLimiter =>
            {
                concurrencyLimiter.PermitLimit = concurrentRequestsLimit.Value;
            });
        });

        return builder;
    }

    /// <summary>
    /// Enables sending traces, metrics and logs to OpenTelemetry endpoint.
    /// Configured via environment variables https://opentelemetry.io/docs/languages/sdk-configuration/general/
    /// Necessary variables: OTEL_EXPORTER_OTLP_ENDPOINT, OTEL_SERVICE_NAME
    /// </summary>
    /// <param name="builder">WebApplicationBuilder</param>
    /// <param name="addressesExcludedFromTraces">Addresses, traces for which will not be sent</param>
    public static WebApplicationBuilder AddTelemetry(this WebApplicationBuilder builder)
    {
        // Setup logging to be exported via OpenTelemetry
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var otelBuilder = builder.Services.AddOpenTelemetry();

        // Add Metrics for ASP.NET Core and our custom metrics and export via OTLP
        otelBuilder.WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation();
            metrics.AddRuntimeInstrumentation();
            metrics.AddHttpClientInstrumentation();
        });

        // Add Tracing for ASP.NET Core and our custom ActivitySource and export via OTLP
        otelBuilder.WithTracing(tracing =>
        {
            tracing.AddSource(builder.Environment.ApplicationName);
            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation();
        });

        // Export OpenTelemetry data via OTLP, using env vars for the configuration
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (otlpEndpoint != null)
        {
            otelBuilder.UseOtlpExporter();
        }

        return builder;
    }
}
