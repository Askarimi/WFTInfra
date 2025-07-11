using AutoMapper;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Application.Services.UserManagment
{
    public partial class UserPasswordService : IUserPasswordService
    {
        #region ctor
        private readonly IRepository<UserPassword> _userPasswordRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;
        public UserPasswordService(
            IRepository<UserPassword> userPasswordRepository,
            IMapper mapper,
            IPasswordHasher passwordHasher)
        {
            _userPasswordRepository = userPasswordRepository;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }

        #endregion

        public async Task<UserPasswordDto> CreatePassword(UserPasswordDto dto)
        {
            var userPassword = _mapper.Map<UserPassword>(dto);

            await _userPasswordRepository.AddAsync(userPassword);

            return _mapper.Map<UserPasswordDto>(userPassword);
        }

        public async Task<UserPasswordDto> GetPasswordByUserIdAsync(long userId)
        {
            var userPassword = await _userPasswordRepository.GetByExpressionAsync(up => up.UserId == userId);

            return _mapper.Map<UserPasswordDto>(userPassword);
        }
    }
}
