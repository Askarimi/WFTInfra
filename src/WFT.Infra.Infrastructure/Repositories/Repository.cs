using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities;
using WFT.Infra.Infrastructure.Data;

namespace WFT.Infra.Infrastructure.Repositories
{
    public partial class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public virtual async Task<TEntity> GetByIdAsync(long id)
        {
            return await _dbSet.FindAsync(id);
        }
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        // پیاده‌سازی متد صفحه‌بندی (Pagination)
        public async Task<IPagedList<TEntity>> GetPagedAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            int pageNumber = 1,
            int pageSize = 10
        )
        {
            var query = _context.Set<TEntity>().AsQueryable();

            // اعمال فیلتر در صورت وجود
            if (filter != null)
                query = query.Where(filter);

            // تعداد کل رکوردها
            var total = await query.CountAsync();

            // اعمال صفحه‌بندی و دریافت داده‌ها
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();  // اینجا داده‌ها رو به لیست تبدیل می‌کنیم

            // بازگشت داده‌ها در قالب PagedList
            return new PagedList<TEntity>(items, total, pageNumber, pageSize);
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);

            await _context.SaveChangesAsync();

            return entity;
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentException("Entities cannot be null or empty");

            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }


        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var set = _context.Set<TEntity>();

            // بررسی وضعیت tracked
            var trackedEntity = _context.ChangeTracker.Entries<TEntity>()
                                        .FirstOrDefault(e => e.Entity.Id == entity.Id)?.Entity;

            if (trackedEntity != null)
            {
                _context.Entry(trackedEntity).CurrentValues.SetValues(entity);
                entity = trackedEntity; // استفاده از همان tracked entity
            }
            else
            {
                var existingEntity = await set.FindAsync(entity.Id);
                if (existingEntity == null)
                    throw new Exception("Entity not found.");

                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                entity = existingEntity;
            }

            try
            {
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception("این رکورد توسط کاربر دیگری ویرایش یا حذف شده است.");
            }
        }


        public virtual async Task<TEntity> UpdateAsynccccc(TEntity entity)
        {

            // ابتدا رکورد موجود را با شناسه آن پیدا می‌کنیم
            var existingEntity = await _context.Set<TEntity>().FindAsync(entity.Id);

            if (existingEntity == null)
                throw new Exception("Entity not found.");

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);

            try
            {

                await _context.SaveChangesAsync();

                return existingEntity;
            }
            catch (DbUpdateConcurrencyException ex)
            {

                throw new Exception("این رکورد توسط کاربر دیگری ویرایش یا حذف شده است.");
            }
        }

        public virtual async Task DeleteAsync(long id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentException("Entities cannot be null or empty");

            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<bool> ExistsAsync(long id)
        {
            return await _dbSet.AnyAsync(e => e.Id.Equals(id));
        }

        public virtual async Task<long> CountAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(predicate);
        }

        // متد کمکی جدید که Expression را دریافت می‌کند و لیست را برمی‌گرداند
        public virtual async Task<IEnumerable<TEntity>> GetListByExpressionAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        public virtual async Task<TEntity> GetByExpressionAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // متد Table برای دسترسی به داده‌ها با استفاده از IQueryable
        public IQueryable<TEntity> Table => _dbSet.AsQueryable();

        // متد TableNoTracking برای بهبود عملکرد در خواندن داده‌ها
        public IQueryable<TEntity> TableNoTracking => _dbSet.AsNoTracking().AsQueryable();


    }
}