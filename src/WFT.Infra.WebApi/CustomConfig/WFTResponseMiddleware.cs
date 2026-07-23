using System.Text.Json;

namespace WFT.Infra.WebApi.CustomConfig;

public class WFTResponseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    public WFTResponseMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        try
        {
            await _next(context);

            memStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memStream).ReadToEndAsync();

            object? data = null;
            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                try
                {
                    data = JsonSerializer.Deserialize<object>(responseBody);
                }
                catch
                {
                    data = responseBody;
                }
            }

            // ✅ اگر خود پاسخ قبلاً شامل success/data/statusCode بود دوباره wrap نکن
            if (responseBody.TrimStart().StartsWith("{") &&
                (responseBody.Contains("\"success\"") && responseBody.Contains("\"data\"")))
            {
                // پاسخ از قبل WFTJsonResult بوده → مستقیماً بفرستش
                memStream.Seek(0, SeekOrigin.Begin);
                await memStream.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
                return;
            }

            // در غیر اینصورت، wrap کن
            var wrappedResponse = new WFTJsonResult
            {
                Success = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300,
                StatusCode = context.Response.StatusCode,
                Data = data,
                Message = null
            };

            context.Response.ContentType = "application/json";
            memStream.SetLength(0);
            await JsonSerializer.SerializeAsync(context.Response.Body, wrappedResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            memStream.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            var errorMessage = "خطای داخلی سرور";
            string? fullError = _env.IsDevelopment() ? ex.ToString() : null;

            var wrappedError = WFTJsonResult.Fail(errorMessage, 500, new Exception(fullError ?? errorMessage));
            context.Response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(context.Response.Body, wrappedError, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            context.Response.Body = originalBodyStream;
        }
    }
}
