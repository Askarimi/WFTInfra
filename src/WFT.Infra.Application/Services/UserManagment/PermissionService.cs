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

        public virtual async Task UpdateAsync(PermissionDto dto)
        {
            var existing = await _permissionRepository.GetByIdAsync(dto.Id);
            if (existing == null) throw new Exception("Permission not found");

            _mapper.Map(dto, existing);
            await _permissionRepository.UpdateAsync(existing);
        }

        public virtual async Task<IEnumerable<PermissionDto>> GetActivePermissionsAsync()
        {
            Expression<Func<Permission, bool>> predicate = p => p.IsActive;
            var permissions = await _permissionRepository.GetListByExpressionAsync(predicate);

            return permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.DisplayName
            }).ToList();
        }

        public Task<IPagedList<PermissionDto>> GetPagedListAsync(PagedQueryRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
