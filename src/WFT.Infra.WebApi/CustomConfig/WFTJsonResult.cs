namespace WFT.Infra.WebApi.CustomConfig;
public class WFTJsonResult
{
    public bool Success { get; set; }
    public object? Data { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public int? LogId { get; set; }
    public MetaData? Meta { get; set; }

    // پاسخ موفق
    public static WFTJsonResult Ok(object data, string? message = null, MetaData? meta = null)
    {
        return new WFTJsonResult
        {
            Success = true,
            Data = data,
            Message = message,
            Error = null,
            Meta = meta
        };
    }

    // پاسخ خطا
    public static WFTJsonResult Fail(string error, string? message = null, int? logId = null, MetaData? meta = null)
    {
        return new WFTJsonResult
        {
            Success = false,
            Data = null,
            Message = message,
            Error = error,
            LogId = logId,
            Meta = meta
        };
    }
}