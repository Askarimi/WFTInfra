using AutoMapper;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Contracts.Models;
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
        public async Task<UserPasswordDto> AddAsync(UserPasswordDto dto)
        {

            var salt = _passwordHasher.GenerateSalt();

            var passwordSalt = $"{dto.Password}{salt}";

            var passwordHash = _passwordHasher.HashPassword(passwordSalt);

            dto.PasswordHash = passwordHash;

            dto.PasswordSalt = salt;

            var userPassword = _mapper.Map<UserPassword>(dto);

            await _userPasswordRepository.AddAsync(userPassword);

            return _mapper.Map<UserPasswordDto>(userPassword);
        }


        public async Task<UserPasswordDto> GetPasswordByUserIdAsync(long userId)
        {
            var userPassword = await _userPasswordRepository.GetByExpressionAsync(up => up.UserId == userId);

            return _mapper.Map<UserPasswordDto>(userPassword);
        }

        public Task DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserPasswordDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<UserPasswordDto> GetByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<UserPasswordDto> GetByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IPagedList<UserPasswordDto>> GetPagedListAsync(PagedQueryRequest request)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(UserPasswordDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
