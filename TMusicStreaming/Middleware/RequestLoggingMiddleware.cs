namespace TMusicStreaming.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Incoming request: {Method} {Path}{QueryString} from {RemoteIp}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                context.Connection.RemoteIpAddress);

            if (context.Request.Path.StartsWithSegments("/api/socialauth"))
            {
                _logger.LogInformation("SocialAuth request headers:");
                foreach (var header in context.Request.Headers.Take(10)) 
                {
                    _logger.LogInformation("  {HeaderName}: {HeaderValue}", header.Key, header.Value);
                }
            }

            var originalBodyStream = context.Response.Body;

            try
            {
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                await _next(context);

                _logger.LogInformation("Response: {StatusCode} for {Method} {Path}",
                    context.Response.StatusCode,
                    context.Request.Method,
                    context.Request.Path);

                // Log 404 details
                if (context.Response.StatusCode == 404)
                {
                    _logger.LogWarning("404 Not Found: {Method} {Path}{QueryString}",
                        context.Request.Method,
                        context.Request.Path,
                        context.Request.QueryString);
                }

                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}