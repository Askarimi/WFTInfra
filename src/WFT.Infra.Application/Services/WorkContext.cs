using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFT.Infra.Contracts.Interfaces;
using System.Security.Claims;

namespace WFT.Infra.Application.Services
{
    public partial class WorkContext : IWorkContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public long? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                return userIdClaim != null && long.TryParse(userIdClaim.Value, out var id) ? id : (long?)null;
            }
        }

        public string Username => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        public List<string> Roles =>
            _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value).ToList() ?? new List<string>();

        public List<string> Permissions =>
            _httpContextAccessor.HttpContext?.User?.FindAll("permission")?.Select(c => c.Value).ToList() ?? new List<string>();
    }
}
