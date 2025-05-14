using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
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

        public virtual async Task UpdateAsync(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
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
        // متد عمومی برای بارگذاری داده‌ها با Include های دینامیک
        public async Task<IEnumerable<TEntity>> GetWithIncludesAsync(
       Expression<Func<TEntity, bool>> predicate = null,
       params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            // اضافه کردن شرایط Filter (predicate) اگر نیاز بود
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            // اضافه کردن Include ها به صورت دینامیک
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }


        // متد Table برای دسترسی به داده‌ها با استفاده از IQueryable
        public IQueryable<TEntity> Table => _dbSet.AsQueryable();

        // متد TableNoTracking برای بهبود عملکرد در خواندن داده‌ها
        public IQueryable<TEntity> TableNoTracking => _dbSet.AsNoTracking().AsQueryable();
    }
}