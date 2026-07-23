using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WFT.Infra.Application.Contracts.Common;
using WFT.Infra.Application.Contracts.Exceptions; // اضافه کردن نیم‌اسپیس استثناها

public class WFTResponseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JsonSerializerOptions _jsonOptions;

    public WFTResponseMiddleware(RequestDelegate next)
    {
        _next = next;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public async Task Invoke(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var bodyText = await new StreamReader(responseBody).ReadToEndAsync();
                
                if (!string.IsNullOrWhiteSpace(bodyText) && !bodyText.Contains("\"isSuccess\":"))
                {
                    var rawData = JsonSerializer.Deserialize<JsonElement>(bodyText);
                    object? data = null;
                    object? meta = null;

                    // اگر دیتای برگشتی از کنترلر خودش فیلد Items و Meta داشت (یعنی PagedResponse بوده)
                    if (rawData.ValueKind == JsonValueKind.Object && rawData.TryGetProperty("items", out var itemsProp))
                    {
                        data = JsonSerializer.Deserialize<object>(itemsProp.GetRawText(), _jsonOptions);
                        if (rawData.TryGetProperty("meta", out var metaProp))
                            meta = JsonSerializer.Deserialize<object>(metaProp.GetRawText(), _jsonOptions);
                    }
                    else
                    {
                        data = JsonSerializer.Deserialize<object>(bodyText, _jsonOptions);
                    }

                    var wrappedResponse = ApiResponse<object>.Success(data, "Success", context.Response.StatusCode, meta);

                    context.Response.Body = originalBodyStream;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(wrappedResponse, _jsonOptions));
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            context.Response.Body = originalBodyStream;
            context.Response.ContentType = "application/json";

            // ۱. بررسی اینکه آیا خطا از نوع خطاهای بیزینسی و تعریف شده توسط ماست؟
            if (ex is AppException appEx)
            {
                context.Response.StatusCode = appEx.StatusCode;
                var errorResponse = ApiResponse<object>.Failure(appEx.Message, appEx.StatusCode);
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, _jsonOptions));
            }
            else
            {
                // ۲. خطاهای غیرمنتظره سرور (Uncaught Exceptions)
                // در این حالت برای حفظ امنیت، متن دقیق Exception سیستم را به کاربر نهایی نشان نمی‌دهیم
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                
                // در اینجا می‌توانید سیستم Logging خود را صدا بزنید:
                // _logger.LogError(ex, "System failure occurred.");

                var errorResponse = ApiResponse<object>.Failure("یک خطای داخلی در سرور رخ داده است.", 500);
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, _jsonOptions));
            }
            return;
        }

        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;
    }
}
