using System.Linq.Expressions;

namespace WFT.Infra.Application.Contracts.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        // دریافت موجودیت بر اساس شناسه
        Task<TEntity> GetByIdAsync(long id);
        // دریافت تمام موجودیت‌ها
        Task<IEnumerable<TEntity>> GetAllAsync();
        // اضافه کردن یک موجودیت
        Task<TEntity> AddAsync(TEntity entity);
        // اضافه کردن مجموعه‌ای از موجودیت‌ها
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        // به‌روزرسانی یک موجودیت
        Task UpdateAsync(TEntity entity);
        // حذف یک موجودیت
        Task DeleteAsync(long id);
        // حذف مجموعه‌ای از موجودیت‌ها
        Task RemoveRangeAsync(IEnumerable<TEntity> entities);
        // جستجو با شرایط خاص
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        // بررسی وجود موجودیت با شناسه
        Task<bool> ExistsAsync(long id);
        // شمارش موجودیت‌ها بر اساس شرایط
        Task<long> CountAsync(Expression<Func<TEntity, bool>> predicate = null);
        // متد کمکی جدید که Expression را دریافت می‌کند و لیست را برمی‌گرداند
        Task<IEnumerable<TEntity>> GetListByExpressionAsync(Expression<Func<TEntity, bool>> predicate);
        // دسترسی به تمام داده‌ها به صورت IQueryable
        IQueryable<TEntity> Table { get; }
        // دسترسی به داده‌ها بدون Tracking (بدون ذخیره‌سازی وضعیت داده‌ها)
        IQueryable<TEntity> TableNoTracking { get; }
    }
}