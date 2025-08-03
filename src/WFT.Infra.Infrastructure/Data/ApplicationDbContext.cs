using Microsoft.EntityFrameworkCore;
using WFT.Infra.Core.Entities;
using WFT.Infra.Core.Entities.UserManagment;

namespace WFT.Infra.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Country> Countries { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserPassword> UserPasswords { get; set; }
        
        // ABAC Tables
        public DbSet<AttributeGroup> AttributeGroups { get; set; }
        public DbSet<AttributeDefinition> AttributeDefinitions { get; set; }
        public DbSet<AttributeValue> AttributeValues { get; set; }
        public DbSet<PolicyRule> PolicyRules { get; set; }
        public DbSet<PolicyCondition> PolicyConditions { get; set; }
        public DbSet<ConditionOperator> ConditionOperators { get; set; }
        public DbSet<RolePolicyRule> RolePolicyRules { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from the current assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}