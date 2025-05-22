using WFT.Infra.Application.Contracts.DTOs.UserManagment;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;

namespace WFT.Infra.Application.InitialData
{
    public partial class InitialDataSeeder
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthService _authService;

        public InitialDataSeeder(IUserService userService,
            IRoleService roleService,
            IPermissionService permissionService,
            IPasswordHasher passwordHasher,
            IAuthService authService)
        {
            _userService = userService;
            _roleService = roleService;
            _permissionService = permissionService;
            _passwordHasher = passwordHasher;
            _authService = authService;
        }

        public async Task SeedAsync()
        {
            // 1. ساخت نقش Admin

            var adminRole = await _roleService.GetByNameAsync("Admin");

            if (adminRole == null)
            {
                var roleDto = new RoleDto
                {
                    Name = "Admin",
                    IsActive = true,
                    Description = "مدیر کل سیستم"
                };

                adminRole = await _roleService.AddAsync(roleDto);

                // 2. ساخت پرمیژن‌ها
                var permissions = new[]
                {
                    "user.create", "user.edit", "user.delete",
                    "role.manage", "permission.manage"
                };

                foreach (var permissionName in permissions)
                {

                    var permission = await _permissionService.GetByNameAsync(permissionName);

                    if (permission == null)
                    {
                        permission = new PermissionDto
                        {
                            Name = permissionName,
                            DisplayName = permissionName,
                            RoleId = adminRole.Id,
                            Description = permissionName
                        };

                        var addedPermission = await _permissionService.AddAsync(permission);
                        await _roleService.AddPermissionsToRoleAsync(adminRole.Id, new List<long> { addedPermission.Id });
                    }
                }

                // 3. ساخت کاربر Admin
                var existingUser = await _userService.GetByUsernameAsync("Admin");

                if (existingUser == null)
                {
                    //var hashedPassword = _passwordHasher.HashPassword();

                    var user = new UserRegisterDto
                    {
                        FirstName = "Super",
                        LastName = "Admin",
                        Username = "SuperAdmin",
                        Password = "Admin123!",
                        Email = "admin@example.com"
                    };

                    var userId = await _userService.RegisterByUserAsync(user);

                    await _userService.AddRoleToUserAsync(userId, new List<long> { adminRole.Id });
                }
            }
        }

    }
}
