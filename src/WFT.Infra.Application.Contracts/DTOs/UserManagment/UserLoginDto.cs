using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record UserLoginDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
