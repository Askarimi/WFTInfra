using Microsoft.EntityFrameworkCore;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Infrastructure.Data;

namespace WFT.Infra.Infrastructure
{
    public partial class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly ApplicationDbContext _context;

        public DatabaseInitializer(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Initialize()
        {
            _context.Database.Migrate();
        }
    }
}
