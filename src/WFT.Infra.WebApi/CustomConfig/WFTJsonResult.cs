using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WFT.Infra.WebApi.CustomConfig
{
    /// <summary>
    /// Unified, flat JSON ActionResult for all APIs.
    /// Produces one-layer JSON with core fields + optional meta for pagination.
    /// </summary>
    public class WFTJsonResult : ActionResult
    {
        public int StatusCode { get; set; } = 200;
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public int? LogId { get; set; }
        public object? Data { get; set; }
        public object? Meta { get; set; }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = StatusCode;
            response.ContentType = "application/json; charset=utf-8";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var output = new
            {
                success = Success,
                statusCode = StatusCode,
                message = Message,
                logId = LogId,
                meta = Meta,
                data = Data
            };

            var json = JsonSerializer.Serialize(output, jsonOptions);
            await response.WriteAsync(json);
        }

        public static WFTJsonResult Ok(object? data = null, object? meta = null, string? message = null)
        {
            return new WFTJsonResult
            {
                Success = true,
                StatusCode = 200,
                Message = message,
                Data = data,
                Meta = meta
            };
        }

        public static WFTJsonResult Fail(string message, int statusCode = 500, Exception? ex = null)
        {
            return new WFTJsonResult
            {
                Success = false,
                StatusCode = statusCode,
                Message = ex != null ? $"{message}: {ex.Message}" : message,
                Data = null
            };
        }
    }
}
