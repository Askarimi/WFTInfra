using AutoMapper;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services.UserManagment
{
    public partial class UserService : IUserService
    {

        #region ctor

        private readonly IRepository<User> _userRepository;

        private readonly IMapper _mapper;


        public UserService(IRepository<User> userRepository, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }
        #endregion

        public Task AssignRolesAsync(long userId, List<long> roleIds)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<UserDto> AddAsync(UserDto dto)
        {
            var user = _mapper.Map<User>(dto);
            await _userRepository.AddAsync(user);
            return _mapper.Map<UserDto>(user);
        }

        public virtual async Task DeleteAsync(long id)
        {
            await _userRepository.DeleteAsync(id);
        }

        public virtual async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public virtual async Task<UserDto> GetByIdAsync(long id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return _mapper.Map<UserDto>(user);
        }

        public virtual async Task UpdateAsync(UserDto dto)
        {
            var user = _mapper.Map<User>(dto);
            await _userRepository.UpdateAsync(user);
        }
    }
}
