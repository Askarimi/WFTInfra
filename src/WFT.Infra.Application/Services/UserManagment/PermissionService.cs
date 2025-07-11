using AutoMapper;
using System.Linq.Expressions;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services.UserManagment
{
    public partial class PermissionService : IPermissionService
    {

        #region ctor

        private readonly IRepository<Permission> _permissionRepository;

        private readonly IMapper _mapper;

        public PermissionService(IRepository<Permission> persmissionRepository, IMapper mapper)
        {
            _mapper = mapper;
            _permissionRepository = persmissionRepository;
        }
        #endregion

        public virtual async Task<PermissionDto> AddAsync(PermissionDto dto)
        {
            var entity = _mapper.Map<Permission>(dto);
            await _permissionRepository.AddAsync(entity);
            return _mapper.Map<PermissionDto>(entity);
        }

        public virtual async Task DeleteAsync(long id)
        {
            await _permissionRepository.DeleteAsync(id);
        }

        public virtual async Task<IEnumerable<PermissionDto>> GetAllAsync()
        {
            var permissions = await _permissionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
        }

        public virtual async Task<PermissionDto> GetByIdAsync(long id)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            return _mapper.Map<PermissionDto>(permission);
        }

        public virtual async Task<PermissionDto> GetByNameAsync(string name)
        {
            var permission = await _permissionRepository.GetByExpressionAsync(p => p.Name.Contains(name));

            return _mapper.Map<PermissionDto>(permission);
        }

        public virtual async Task<PermissionDto> UpdateAsync(PermissionDto dto)
        {
            var existing = await _permissionRepository.GetByIdAsync(dto.Id);

            if (existing == null) throw new Exception("Permission not found");

            _mapper.Map(dto, existing);

            await _permissionRepository.UpdateAsync(existing);

            return _mapper.Map<PermissionDto>(existing);
        }

        public virtual async Task<IEnumerable<PermissionDto>> GetActivePermissionsAsync()
        {
            Expression<Func<Permission, bool>> predicate = p => p.IsActive;
            var permissions = await _permissionRepository.GetListByExpressionAsync(predicate);

            return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
        }

        public async Task<IPagedList<PermissionDto>> GetPagedListAsync(PagedQueryRequest request)
        {
            // شروع از یک فیلتر پایه برای جستجوی عمومی
            Expression<Func<Permission, bool>> filter = permission =>
                string.IsNullOrEmpty(request.SearchTerm) ||
                permission.Name.Contains(request.SearchTerm) ||
                permission.DisplayName.Contains(request.SearchTerm);

            // اگر فیلترهای اضافی (Filters) وجود دارند، آن‌ها را اضافه می‌کنیم
            if (request.Filters != null && request.Filters.Count > 0)
            {
                foreach (var filterItem in request.Filters)
                {
                    var property = typeof(Permission).GetProperty(filterItem.Key);
                    if (property != null)
                    {
                        var param = Expression.Parameter(typeof(Permission), "permission");
                        var left = Expression.Property(param, property);
                        var right = Expression.Constant(filterItem.Value);
                        var equalExpression = Expression.Equal(left, right);

                        // ترکیب فیلترهای قبلی با فیلتر جدید
                        filter = Expression.Lambda<Func<Permission, bool>>(Expression.AndAlso(filter.Body, equalExpression), param);
                    }
                }
            }

            // دریافت داده‌ها با صفحه‌بندی
            var result = await _permissionRepository.GetPagedAsync(filter, request.PageNumber, request.PageSize);

            // تبدیل به PermissionDto با استفاده از AutoMapper
            var permissionDtos = _mapper.Map<IEnumerable<PermissionDto>>(result.Items);

            // بازگشت نتایج صفحه‌بندی‌شده
            return new PagedList<PermissionDto>(permissionDtos, result.TotalCount, result.PageNumber, result.PageSize);
        }
    }
}
