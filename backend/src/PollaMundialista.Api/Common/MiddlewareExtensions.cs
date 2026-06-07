namespace PollaMundialista.Api.Common;

public static class MiddlewareExtensions
{
    /// <summary>Adds conservative security headers to every response.</summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (ctx, next) =>
        {
            var h = ctx.Response.Headers;
            h["X-Content-Type-Options"] = "nosniff";
            h["X-Frame-Options"] = "DENY";
            h["Referrer-Policy"] = "no-referrer";
            h["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            await next();
        });

    /// <summary>
    /// Ensures every request carries a correlation id (from the inbound header or
    /// generated), echoes it back, and adds it to the logging scope.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        const string header = "X-Correlation-ID";
        return app.Use(async (ctx, next) =>
        {
            var correlationId = ctx.Request.Headers.TryGetValue(header, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value.ToString()
                : ctx.TraceIdentifier;

            ctx.Response.Headers[header] = correlationId;

            var logger = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Request");
            using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
            {
                await next();
            }
        });
    }
}