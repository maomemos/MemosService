using MemosService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MemosService.Data
{
    public class MemosContext : DbContext
    {
        protected readonly IConfiguration Configuration;
        public DbSet<Memo> Memos { get; set; }
        public DbSet<User> Users { get; set; }

        public MemosContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(Configuration.GetConnectionString("WebApiDatabase"));
        }
    }
}
