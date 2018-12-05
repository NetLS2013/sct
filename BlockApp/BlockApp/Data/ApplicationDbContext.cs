using BlockApp.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlockApp.Models;

namespace BlockApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<MerchantAccount> MerchantAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Subscriptions> Subscriptions { get; set; }
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
