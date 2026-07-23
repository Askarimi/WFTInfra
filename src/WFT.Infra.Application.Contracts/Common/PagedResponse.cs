public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; }
    public object Meta { get; set; }

    public PagedResponse(IEnumerable<T> items, object meta)
    {
        Items = items;
        Meta = meta;
    }
}
