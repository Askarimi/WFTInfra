namespace WFT.Infra.WebApi.CustomConfig;
public class MetaData
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public int? TotalItems { get; set; }
    public int? TotalPages { get; set; }
    public List<string>? Warnings { get; set; }
}
