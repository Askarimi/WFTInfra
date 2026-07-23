namespace WFT.Infra.WebApi.CustomConfig;
public class MetaData
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public int? TotalCount { get; set; }
    public int? TotalPages { get; set; }
    public List<string>? Warnings { get; set; }
}
