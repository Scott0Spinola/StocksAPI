using src.Models;
using Microsoft.EntityFrameworkCore;

namespace src.Data;

public class StocksContext(DbContextOptions<StocksContext> options) : DbContext(options)
{
    public DbSet<Cliente_Movimento> Cliente_Movimentos => Set<Cliente_Movimento>();

    public DbSet<Cliente_Tag> Cliente_Tags => Set<Cliente_Tag>();

    public DbSet<VwClienteMovimento> VwClienteMovimentos => Set<VwClienteMovimento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<VwClienteMovimento>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_ClienteMovimentos", "dbo");
        });
    }
}
 