using AutoMapper;
using WFT.Infra.Application.Contracts.DTOs.RolePermissionManagement;
using WFT.Infra.Application.Contracts.Interfaces.RolePermissionManagement;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services.RolePermissionManagement
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<Role> _roleRepository;
        private readonly IMapper _mapper;

        public RoleService(IRepository<Role> roleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoleDetailDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleDetailDto>>(roles);
        }

        public async Task<RoleDetailDto?> GetRoleByIdAsync(long id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            return role == null ? null : _mapper.Map<RoleDetailDto>(role);
        }

        public async Task<RoleDetailDto> CreateRoleAsync(CreateRoleDto dto)
        {
            var role = new Role
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            await _roleRepository.AddAsync(role);
            return _mapper.Map<RoleDetailDto>(role);
        }

        public async Task<RoleDetailDto> UpdateRoleAsync(UpdateRoleDto dto)
        {
            var role = await _roleRepository.GetByIdAsync(dto.Id);
            if (role == null)
                throw new KeyNotFoundException($"Role with ID {dto.Id} not found");

            role.Name = dto.Name;
            role.Description = dto.Description;
            role.IsActive = dto.IsActive;

            await _roleRepository.UpdateAsync(role);
            return _mapper.Map<RoleDetailDto>(role);
        }

        public async Task<bool> DeleteRoleAsync(long id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                return false;

            await _roleRepository.DeleteAsync(id);
            return true;
        }
    }
}

