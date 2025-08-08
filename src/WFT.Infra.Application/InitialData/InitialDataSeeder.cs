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
        private readonly IAttributeService _attributeService;
        private readonly IConditionOperatorService _conditionOperatorService;
        private readonly IPolicyRuleService _policyRuleService;
        private readonly IAttributeGroupService _attributeGroupService;

        public InitialDataSeeder(IUserService userService,
            IRoleService roleService,
            IPermissionService permissionService,
            IPasswordHasher passwordHasher,
            IAuthService authService,
            IAttributeService attributeService,
            IConditionOperatorService conditionOperatorService,
            IPolicyRuleService policyRuleService,
            IAttributeGroupService attributeGroupService)
        {
            _userService = userService;
            _roleService = roleService;
            _permissionService = permissionService;
            _passwordHasher = passwordHasher;
            _authService = authService;
            _attributeService = attributeService;
            _conditionOperatorService = conditionOperatorService;
            _policyRuleService = policyRuleService;
            _attributeGroupService = attributeGroupService;
        }

        public async Task SeedAsync()
        {
            // 1. Seed Attribute Groups
            await SeedAttributeGroupsAsync();

            // 2. Seed Condition Operators
            await SeedConditionOperatorsAsync();

            // 3. Seed Attribute Definitions
            await SeedAttributeDefinitionsAsync();

            // 3. ساخت نقش Admin
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

                // 4. ساخت پرمیژن‌ها
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

                // 5. ساخت کاربر Admin
                var existingUser = await _userService.GetByUsernameAsync("SuperAdmin");

                if (existingUser == null)
                {
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

                    // 6. Set user attributes for ABAC
                    await SeedUserAttributesAsync(userId);
                }
            }
        }

        private async Task SeedConditionOperatorsAsync()
        {
            var operators = new[]
            {
                new { Name = "Equals", DisplayName = "برابر است با", Symbol = "==", DataTypes = "String,Number,Boolean" },
                new { Name = "Contains", DisplayName = "شامل است", Symbol = "contains", DataTypes = "String" },
                new { Name = "StartsWith", DisplayName = "شروع می‌شود با", Symbol = "startsWith", DataTypes = "String" },
                new { Name = "EndsWith", DisplayName = "پایان می‌یابد با", Symbol = "endsWith", DataTypes = "String" },
                new { Name = "GreaterThan", DisplayName = "بزرگتر از", Symbol = ">", DataTypes = "Number,DateTime" },
                new { Name = "LessThan", DisplayName = "کوچکتر از", Symbol = "<", DataTypes = "Number,DateTime" },
                new { Name = "Between", DisplayName = "بین", Symbol = "between", DataTypes = "Number,DateTime" },
                new { Name = "In", DisplayName = "در", Symbol = "in", DataTypes = "String,Number" }
            };

            foreach (var op in operators)
            {
                var existingOperator = await _conditionOperatorService.GetByNameAsync(op.Name);
                if (existingOperator == null)
                {
                    var operatorDto = new ConditionOperatorDto
                    {
                        Name = op.Name,
                        DisplayName = op.DisplayName,
                        Symbol = op.Symbol,
                        DataTypes = op.DataTypes,
                        IsActive = true
                    };
                    await _conditionOperatorService.AddAsync(operatorDto);
                }
            }
        }

        private async Task SeedAttributeDefinitionsAsync()
        {
            var attributes = new[]
            {
                new { Name = "Department", DisplayName = "دپارتمان", DataType = "String", Source = "User" },
                new { Name = "UserLevel", DisplayName = "سطح کاربر", DataType = "String", Source = "User" },
                new { Name = "DocumentType", DisplayName = "نوع سند", DataType = "String", Source = "Resource" },
                new { Name = "CurrentTime", DisplayName = "زمان فعلی", DataType = "DateTime", Source = "Environment" },
                new { Name = "CurrentDate", DisplayName = "تاریخ فعلی", DataType = "DateTime", Source = "Environment" }
            };

            foreach (var attr in attributes)
            {
                var existingAttribute = await _attributeService.GetByNameAsync(attr.Name);
                if (existingAttribute == null)
                {
                    var attributeDto = new AttributeDefinitionDto
                    {
                        Name = attr.Name,
                        DisplayName = attr.DisplayName,
                        DataType = attr.DataType,
                        Source = attr.Source,
                        IsRequired = false,
                        IsActive = true,
                        AttributeGroupId = 1
                    };
                    await _attributeService.AddAsync(attributeDto);
                }
            }
        }

        private async Task SeedAttributeGroupsAsync()
        {
            var groups = new[]
            {
                new { Name = "user-basic", DisplayName = "اطلاعات پایه کاربر", Description = "ویژگی‌های پایه کاربر مانند نام، ایمیل، و غیره", SortOrder = 1, Icon = "user", Color = "#3B82F6" },
                new { Name = "user-role", DisplayName = "نقش و دسترسی", Description = "ویژگی‌های مربوط به نقش و دسترسی کاربر", SortOrder = 2, Icon = "shield", Color = "#10B981" },
                new { Name = "user-department", DisplayName = "بخش و سازمان", Description = "ویژگی‌های مربوط به بخش و سازمان کاربر", SortOrder = 3, Icon = "building", Color = "#F59E0B" },
                new { Name = "resource-document", DisplayName = "مستندات", Description = "ویژگی‌های مربوط به مستندات و فایل‌ها", SortOrder = 4, Icon = "file-text", Color = "#8B5CF6" },
                new { Name = "resource-project", DisplayName = "پروژه‌ها", Description = "ویژگی‌های مربوط به پروژه‌ها", SortOrder = 5, Icon = "folder", Color = "#EF4444" },
                new { Name = "environment-time", DisplayName = "زمان و تاریخ", Description = "ویژگی‌های مربوط به زمان و تاریخ", SortOrder = 6, Icon = "clock", Color = "#6B7280" }
            };

            foreach (var group in groups)
            {
                var existingGroup = await _attributeGroupService.GetByNameAsync(group.Name);
                if (existingGroup == null)
                {
                    var groupDto = new AttributeGroupDto
                    {
                        Name = group.Name,
                        DisplayName = group.DisplayName,
                        Description = group.Description,
                        SortOrder = group.SortOrder,
                        Icon = group.Icon,
                        Color = group.Color,
                        IsActive = true
                    };

                    await _attributeGroupService.AddAsync(groupDto);
                }
            }
        }

        private async Task SeedUserAttributesAsync(long userId)
        {
            // Set department attribute for admin user
            var departmentAttr = await _attributeService.GetByNameAsync("Department");
            if (departmentAttr != null)
            {
                var departmentValue = new AttributeValueDto
                {
                    AttributeDefinitionId = departmentAttr.Id,
                    UserId = userId,
                    Value = "IT",
                    IsActive = true
                };
                await _attributeService.SetAttributeValueAsync(departmentValue);
            }

            // Set user level attribute for admin user
            var userLevelAttr = await _attributeService.GetByNameAsync("UserLevel");
            if (userLevelAttr != null)
            {
                var userLevelValue = new AttributeValueDto
                {
                    AttributeDefinitionId = userLevelAttr.Id,
                    UserId = userId,
                    Value = "Senior",
                    IsActive = true
                };
                await _attributeService.SetAttributeValueAsync(userLevelValue);
            }
        }

    }
}
