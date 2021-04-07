using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ICWebAPI.Data
{
    public class ICContext : IdentityDbContext
    {
        public ICContext(DbContextOptions<ICContext> options) : base(options) { }
    }
}