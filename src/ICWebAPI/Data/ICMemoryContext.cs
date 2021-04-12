using ICWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ICWebAPI.Data
{
    public class ICMemoryContext : DbContext
    {
        public ICMemoryContext(DbContextOptions<ICMemoryContext> options) : base(options)
        { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}