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

        // IServiceBase implementation
        public async Task<UserPasswordDto> GetByIdAsync(long id)
        {
            var userPassword = await _userPasswordRepository.GetByIdAsync(id);
            return _mapper.Map<UserPasswordDto>(userPassword);
        }

        public async Task<IEnumerable<UserPasswordDto>> GetAllAsync()
        {
            var userPasswords = await _userPasswordRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserPasswordDto>>(userPasswords);
        }

        public async Task<IPagedList<UserPasswordDto>> GetPagedListAsync(PagedQueryRequest request)
        {
            Expression<Func<UserPassword, bool>> filter = userPassword =>
                string.IsNullOrEmpty(request.SearchTerm) ||
                userPassword.UserId.ToString().Contains(request.SearchTerm);

            if (request.Filters != null && request.Filters.Count > 0)
            {
                foreach (var filterItem in request.Filters)
                {
                    var property = typeof(UserPassword).GetProperty(filterItem.Key);
                    if (property != null)
                    {
                        var param = Expression.Parameter(typeof(UserPassword), "userPassword");
                        var left = Expression.Property(param, property);
                        var right = Expression.Constant(filterItem.Value);
                        var equalExpression = Expression.Equal(left, right);

                        filter = Expression.Lambda<Func<UserPassword, bool>>(Expression.AndAlso(filter.Body, equalExpression), param);
                    }
                }
            }

            var result = await _userPasswordRepository.GetPagedAsync(filter, request.PageNumber, request.PageSize);
            var userPasswordDtos = _mapper.Map<IEnumerable<UserPasswordDto>>(result.Items);

            return new PagedList<UserPasswordDto>(userPasswordDtos, result.TotalCount, result.PageNumber, result.PageSize);
        }

        public async Task<UserPasswordDto> GetByNameAsync(string name)
        {
            // UserPassword doesn't have a name property, so we'll return null
            return null;
        }

        public async Task<UserPasswordDto> AddAsync(UserPasswordDto dto)
        {
            var userPassword = _mapper.Map<UserPassword>(dto);
            await _userPasswordRepository.AddAsync(userPassword);
            return _mapper.Map<UserPasswordDto>(userPassword);
        }

        public async Task<UserPasswordDto> UpdateAsync(UserPasswordDto dto)
        {
            var userPassword = _mapper.Map<UserPassword>(dto);
            await _userPasswordRepository.UpdateAsync(userPassword);
            return _mapper.Map<UserPasswordDto>(userPassword);
        }

        public async Task DeleteAsync(long id)
        {
            await _userPasswordRepository.DeleteAsync(id);
        }

        public async Task<bool> SetPasswordAsync(UserPasswordDto request)
        {
            // Check if user already has a password
            var existingPassword = await _userPasswordRepository.GetByExpressionAsync(up => up.UserId == request.UserId);
            
            if (existingPassword != null)
            {
                // Update existing password
                existingPassword.PasswordHash = _passwordHasher.HashPassword(request.Password);
                existingPassword.LastChangedAt = DateTime.UtcNow;
                await _userPasswordRepository.UpdateAsync(existingPassword);
            }
            else
            {
                // Create new password
                var userPassword = new UserPassword
                {
                    UserId = request.UserId,
                    PasswordHash = _passwordHasher.HashPassword(request.Password),
                    LastChangedAt = DateTime.UtcNow
                };
                await _userPasswordRepository.AddAsync(userPassword);
            }

            return true;
        }

        public async Task<bool> ChangePasswordAsync(UserPasswordDto request)
        {
            // Check if user has an existing password
            var existingPassword = await _userPasswordRepository.GetByExpressionAsync(up => up.UserId == request.UserId);
            
            if (existingPassword == null)
            {
                throw new InvalidOperationException("User does not have an existing password to change.");
            }

            // Update the password
            existingPassword.PasswordHash = _passwordHasher.HashPassword(request.Password);
            existingPassword.LastChangedAt = DateTime.UtcNow;
            await _userPasswordRepository.UpdateAsync(existingPassword);

            return true;
        }

        public async Task<UserPasswordAuthDto> GetPasswordForAuthAsync(long userId)
        {
            var userPassword = await _userPasswordRepository.GetByExpressionAsync(up => up.UserId == userId);
            
            if (userPassword == null)
                return null;

            return new UserPasswordAuthDto
            {
                UserId = userPassword.UserId,
                PasswordHash = userPassword.PasswordHash,
                LastChangedAt = userPassword.LastChangedAt
            };
        }
    }
}
