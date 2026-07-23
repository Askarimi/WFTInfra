using AutoMapper;
using WFT.Infra.Application.Contracts.DTOs.RolePermissionManagement;
using WFT.Infra.Application.Contracts.Interfaces.RolePermissionManagement;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services.RolePermissionManagement
{
    public class PermissionService : IPermissionService
    {
        private readonly IRepository<Permission> _permissionRepository;
        private readonly IMapper _mapper;

        public PermissionService(IRepository<Permission> permissionRepository, IMapper mapper)
        {
            _permissionRepository = permissionRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PermissionDetailDto>> GetAllPermissionsAsync()
        {
            var permissions = await _permissionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PermissionDetailDto>>(permissions);
        }

        public async Task<PermissionDetailDto?> GetPermissionByIdAsync(long id)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            return permission == null ? null : _mapper.Map<PermissionDetailDto>(permission);
        }

        public async Task<PermissionDetailDto> CreatePermissionAsync(CreatePermissionDto dto)
        {
            var permission = new Permission
            {
                Name = dto.Name,
                DisplayName = dto.DisplayName,
                IsActive = dto.IsActive
            };

            await _permissionRepository.AddAsync(permission);
            return _mapper.Map<PermissionDetailDto>(permission);
        }

        public async Task<PermissionDetailDto> UpdatePermissionAsync(UpdatePermissionDto dto)
        {
            var permission = await _permissionRepository.GetByIdAsync(dto.Id);
            if (permission == null)
                throw new KeyNotFoundException($"Permission with ID {dto.Id} not found");

            permission.Name = dto.Name;
            permission.DisplayName = dto.DisplayName;
            permission.IsActive = dto.IsActive;

            await _permissionRepository.UpdateAsync(permission);
            return _mapper.Map<PermissionDetailDto>(permission);
        }

        public async Task<bool> DeletePermissionAsync(long id)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null)
                return false;

            await _permissionRepository.DeleteAsync(id);
            return true;
        }
    }
}

