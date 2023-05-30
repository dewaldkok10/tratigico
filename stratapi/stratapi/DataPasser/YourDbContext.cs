using Microsoft.EntityFrameworkCore;
using stratapi.DatabaseTablesM;

public class StratDbContext : DbContext
{
    public StratDbContext(DbContextOptions<StratDbContext> options) : base(options)
    {
    }

    public DbSet<Login> Logins { get; set; }
    public DbSet<UserDetails> UserDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Login>()
            .HasOne(l => l.UserDetails)
            .WithOne(ud => ud.Login)
            .HasForeignKey<UserDetails>(ud => ud.LoginId);

        base.OnModelCreating(modelBuilder);
    }
}