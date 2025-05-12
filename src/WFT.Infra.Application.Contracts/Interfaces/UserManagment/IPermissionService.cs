using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Interfaces.UserManagment
{
    public  interface IPermissionService:IServiceBase<PermissionDto>
    {
        Task<IEnumerable<PermissionDto>> GetActivePermissionsAsync();
    }
}
