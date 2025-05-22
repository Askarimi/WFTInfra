using AutoMapper;
using WFT.Infra.Application.Contracts.DTOs;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities;

namespace WFT.Infra.Application.Services
{
    public partial class RefreshTokenService : IRefreshTokenService
    {
        #region ctor

        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly IMapper _mapper;
        public RefreshTokenService(IRepository<RefreshToken> refreshTokenRepository,
            IMapper mapper
            )
        {
            _refreshTokenRepository = refreshTokenRepository;
            _mapper = mapper;
        }

        #endregion

        public async Task AddAsync(RefreshTokenDto dto)
        {
            var refreshToken = _mapper.Map<RefreshToken>(dto);

            await _refreshTokenRepository.AddAsync(refreshToken);

        }

        public async Task<RefreshTokenDto?> GetByTokenAsync(string token)
        {
            var refreshToken = await _refreshTokenRepository.GetByExpressionAsync(r => r.Token == token);

            return _mapper.Map<RefreshTokenDto?>(refreshToken);
        }


        public async Task<bool> IsValidAsync(string token)
        {
            var rt = await GetByTokenAsync(token);
            return rt is { IsRevoked: false } && rt.ExpiresAt > DateTime.UtcNow;
        }

        public async Task RevokeAsync(string token)
        {
            var rt = await GetByTokenAsync(token);

            if (rt != null)
            {
                rt.IsRevoked = true;

                var refreshToken = _mapper.Map<RefreshToken>(rt);

                await _refreshTokenRepository.UpdateAsync(refreshToken);
            }
        }
    }
}
