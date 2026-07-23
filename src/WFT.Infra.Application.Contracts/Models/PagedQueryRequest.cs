namespace WFT.Infra.Application.Contracts.Models
{
    public partial class PagedQueryRequest
    {
        // کلمه جستجو عمومی برای فیلتر کردن داده‌ها
        public string? SearchTerm { get; set; }

        // فیلترهای اضافی به صورت دیکشنری (کلید = فیلد، مقدار = مقدار فیلتر)
        public Dictionary<string, object>? Filters { get; set; }

        // شماره صفحه برای صفحه‌بندی
        public int PageNumber { get; set; } = 1;

        // تعداد رکوردها در هر صفحه
        public int PageSize { get; set; } = 10;

        // فیلد مرتب‌سازی (اختیاری)
        public string? SortBy { get; set; }

        // جهت مرتب‌سازی (اختیاری)
        public SortDirectionEnum? SortDirection { get; set; } = SortDirectionEnum.Ascending;

        public enum SortDirectionEnum
        {
            Ascending,
            Descending
        }
    }
}
