using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces
{
    public partial interface IJwtTokenGenerator
    {
        public string GenerateToken(string userId, string username, IEnumerable<string> roles, IEnumerable<string> permissions);
    }
}
