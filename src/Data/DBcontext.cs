using src.Models;
using Microsoft.EntityFrameworkCore;

namespace src.Data;

public class StocksContext(DbContextOptions<StocksContext> options) : DbContext(options)
{
    public DbSet<Cliente_Movimento> Cliente_Movimentos => Set<Cliente_Movimento>();

    public DbSet<Cliente_Tag> Cliente_Tags => Set<Cliente_Tag>();

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Cliente_Faturas> Cliente_Faturas => Set<Cliente_Faturas>();

    public DbSet<Cliente_DetalheFaturas> Cliente_DetalheFaturas => Set<Cliente_DetalheFaturas>();

    public DbSet<Cliente_Guias> Cliente_Guias => Set<Cliente_Guias>();

    public DbSet<Cliente_DetalheGuias> Cliente_DetalheGuias => Set<Cliente_DetalheGuias>();

    public DbSet<Entidade> Entidades => Set<Entidade>();

    public DbSet<VwClienteMovimento> VwClienteMovimentos => Set<VwClienteMovimento>();

    public DbSet<VwProximaEntrega> VwProximasEntregas => Set<VwProximaEntrega>();

    public DbSet<VwIntervencoesAlertasPorHotel> VwIntervencoesAlertasPorHotel => Set<VwIntervencoesAlertasPorHotel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente_Faturas>(entity =>
        {
            entity.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            entity.Property(x => x.NumeroDoc)
                .HasMaxLength(255);
            entity.HasAlternateKey(x => x.NumeroDoc);
        });

        modelBuilder.Entity<Cliente_DetalheFaturas>(entity =>
        {
            entity.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            entity.HasIndex(x => x.DocId);

            entity.HasOne<Cliente_Faturas>()
                .WithMany()
                .HasForeignKey(x => x.DocId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.NumeroDoc)
                .HasMaxLength(255);

            entity.HasOne<Cliente_Faturas>()
                .WithMany()
                .HasForeignKey(x => x.NumeroDoc)
                .HasPrincipalKey(x => x.NumeroDoc)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente_Guias>(entity =>
        {
            entity.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            entity.Property(x => x.NumeroGuia)
                .HasMaxLength(255);
            entity.HasAlternateKey(x => x.NumeroGuia);
        });

        modelBuilder.Entity<Cliente_DetalheGuias>(entity =>
        {
            entity.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            entity.HasIndex(x => x.DocId);

            entity.HasOne<Cliente_Guias>()
                .WithMany()
                .HasForeignKey(x => x.DocId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.NumeroGuia)
                .HasMaxLength(255);

            entity.HasOne<Cliente_Guias>()
                .WithMany()
                .HasForeignKey(x => x.NumeroGuia)
                .HasPrincipalKey(x => x.NumeroGuia)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            // Relationships to Cliente_Tag and Cliente_Movimento are based on the business key `Nome`.
            // SQL Server requires the referenced column to be a key (PK/AK) and have a bounded length.
            entity.Property(x => x.Nome).HasMaxLength(255);
            entity.HasAlternateKey(x => x.Nome);

            entity.HasOne(x => x.Entidade)
                .WithMany(x => x.Clientes)
                .HasForeignKey(x => x.IdEntidade)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente_Tag>(entity =>
        {
            entity.Property(x => x.Unidade).HasMaxLength(255);

            // FK: Cliente_Tags.Unidade -> Clientes.Nome
            entity.HasOne(x => x.Nome)
                .WithMany()
                .HasForeignKey(x => x.Unidade)
                .HasPrincipalKey(x => x.Nome)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente_Movimento>(entity =>
        {
            entity.Property(x => x.Cliente).HasMaxLength(255);

            // FK: Cliente_Movimentos.Cliente -> Clientes.Nome
            entity.HasOne(x => x.Nome)
                .WithMany()
                .HasForeignKey(x => x.Cliente)
                .HasPrincipalKey(x => x.Nome)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VwClienteMovimento>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_ClienteMovimentos", "dbo");
        });

        modelBuilder.Entity<VwProximaEntrega>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_ProximasEntregas", "dbo");
        });

        modelBuilder.Entity<VwIntervencoesAlertasPorHotel>(entity =>
        {
            // Backed by a view; the key is for EF tracking/testing convenience.
            entity.HasKey(x => x.Hotel);
            entity.ToView("vw_IntervencoesAlertasPorHotel", "dbo");
        });
    }
}

 