using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Core.Entities.UserManagment;
using WFT.Infra.Infrastructure.Data;

namespace WFT.Infra.Infrastructure.Repositories
{
    public partial class UserRepository : Repository<User>, IUserRepository<User>
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByIdWithRolesAndPermissionsAsync(long userId)
        {
            var user = await GetWithIncludesAsync(
                               u => u.Id == userId,
                               u => u.UserRoles,
                               u => u.UserRoles.Select(ur => ur.Role),
                               u => u.UserRoles.Select(ur => ur.Role.RolePermissions),
                               u => u.UserRoles.Select(ur => ur.Role.RolePermissions.Select(rp => rp.Permission))
                                );
            return user.FirstOrDefault();
        }
    }
}
