using System.Collections;
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
                    data = SafeDeserialize(responseBody);
                }
                catch
                {
                    data = responseBody;
                }
            }

            // Handle empty body for 204, 401, 403
            if (string.IsNullOrWhiteSpace(responseBody) &&
                (context.Response.StatusCode == StatusCodes.Status204NoContent ||
                 context.Response.StatusCode == StatusCodes.Status401Unauthorized ||
                 context.Response.StatusCode == StatusCodes.Status403Forbidden))
            {
                data = null;
            }

            // Check if data is already an ApiEnvelope or PagedResponse (has Success, Data, Meta properties)
            // If so, return it directly without double-wrapping in WFTJsonResult
            if (data != null && data.GetType().IsGenericType)
            {
                var typeName = data.GetType().GetGenericTypeDefinition().Name;
                if (typeName.Contains("ApiEnvelope") || typeName.Contains("PagedResponse"))
                {
                    // ApiEnvelope/PagedResponse already has Success, Data, Meta, Message, Error
                    // Return it directly without wrapping in WFTJsonResult
                    context.Response.ContentType = "application/json";
                    memStream.SetLength(0);
                    await JsonSerializer.SerializeAsync(context.Response.Body, data, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                        WriteIndented = true,
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                    });

                    memStream.Seek(0, SeekOrigin.Begin);
                    await memStream.CopyToAsync(originalBodyStream);
                    context.Response.Body = originalBodyStream;
                    return;
                }
            }

            string? errorMessage = null;
            string? userMessage = null;

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                errorMessage = "کاربر احراز هویت نشده است.";
            }
            else if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                errorMessage = "شما مجوز دسترسی به این بخش را ندارید.";
            }
            else if (context.Response.StatusCode >= 400)
            {
                errorMessage = "خطایی رخ داده است.";
            }

            var wrappedResponse = new WFTJsonResult
            {
                Success = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300,
                Data = data,
                Message = userMessage,
                Error = errorMessage,
                LogId = null,
                Meta = null
            };

            context.Response.ContentType = "application/json";
            memStream.SetLength(0);
            await JsonSerializer.SerializeAsync(context.Response.Body, wrappedResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase, // Use camelCase for JSON:API conventions
                WriteIndented = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });

            memStream.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            var errorMessage = "Internal Server Error";
            string? fullError = _env.IsDevelopment() ? GetExceptionDetails(ex) : null;

            var wrappedError = WFTJsonResult.Fail(errorMessage, fullError);
            context.Response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(context.Response.Body, wrappedError, new JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase, // Use camelCase for JSON:API conventions
                WriteIndented = true
            });

            context.Response.Body = originalBodyStream;
        }
    }

    private object SafeDeserialize(string responseBody)
    {
        var obj = JsonSerializer.Deserialize<object>(responseBody);

        if (obj is IDictionary<string, object> dict)
        {
            var cleanDict = dict
                .Where(kv => !IsEfCoreProxy(kv.Value))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            return cleanDict;
        }

        return obj;
    }

    private bool IsEfCoreProxy(object? obj)
    {
        if (obj == null) return false;
        var type = obj.GetType();
        return type.Namespace?.StartsWith("System.Data.Entity.DynamicProxies") == true
               || type.Name.EndsWith("Proxy");
    }

    private string GetExceptionDetails(Exception ex)
    {
        var builder = new System.Text.StringBuilder();
        WriteExceptionDetails(ex, builder, 0);
        return builder.ToString();
    }

    private void WriteExceptionDetails(Exception exception, System.Text.StringBuilder builderToFill, int level)
    {
        var indent = new string(' ', level * 2);
        if (level > 0) builderToFill.AppendLine(indent + "=== INNER EXCEPTION ===");

        builderToFill.AppendLine($"{indent}Message: {exception.Message}");
        builderToFill.AppendLine($"{indent}StackTrace: {exception.StackTrace}");

        foreach (DictionaryEntry de in exception.Data)
        {
            builderToFill.AppendLine($"{indent}{de.Key} = {de.Value}");
        }

        if (exception.InnerException != null)
            WriteExceptionDetails(exception.InnerException, builderToFill, level + 1);
    }
}
