namespace WFT.Infra.Application.Contracts.Interfaces
{
    public partial interface IPagedList<TEntity>
    {
        // داده‌های صفحه جاری
        IEnumerable<TEntity> Items { get; }

        // تعداد کل رکوردها
        int TotalCount { get; }

        // شماره صفحه جاری
        int PageNumber { get; }

        // تعداد آیتم‌ها در هر صفحه
        int PageSize { get; }

        // تعداد کل صفحات
        int TotalPages { get; }

        // بررسی اینکه آیا صفحه بعدی وجود دارد
        bool HasNextPage { get; }

        // بررسی اینکه آیا صفحه قبلی وجود دارد
        bool HasPreviousPage { get; }
    }
}
