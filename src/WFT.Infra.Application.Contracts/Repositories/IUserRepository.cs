using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFT.Infra.Application.Contracts.DTOs.UserManagment;

namespace WFT.Infra.Application.Contracts.Repositories
{
    public interface IUserRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdWithRolesAndPermissionsAsync(long userId);
    }
}
