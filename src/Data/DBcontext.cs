using src.Models;
using Microsoft.EntityFrameworkCore;

namespace src.Data;

public class StocksContext(DbContextOptions<StocksContext> options) : DbContext(options)
{
    public DbSet<Cliente_Movimento> Cliente_Movimentos => Set<Cliente_Movimento>();

    public DbSet<Cliente_Tag> Cliente_Tags => Set<Cliente_Tag>();

}
