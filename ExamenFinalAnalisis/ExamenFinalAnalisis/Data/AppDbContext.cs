using ExamenFinalAnalisis.Models;
using Microsoft.EntityFrameworkCore;

namespace ExamenFinalAnalisis.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Envio> Envios => Set<Envio>();
    public DbSet<HistorialEstado> Historiales => Set<HistorialEstado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Envio>()
            .HasIndex(e => e.CodigoRastreo)
            .IsUnique();

        modelBuilder.Entity<Envio>()
            .HasOne(e => e.Remitente)
            .WithMany()
            .HasForeignKey(e => e.RemitenteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Envio>()
            .HasOne(e => e.Destinatario)
            .WithMany()
            .HasForeignKey(e => e.DestinatarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Envio>()
            .HasMany(e => e.Historial)
            .WithOne(h => h.Envio!)
            .HasForeignKey(h => h.EnvioId)
            .OnDelete(DeleteBehavior.Cascade);

        // SQLite no tiene tipo decimal nativo: lo mapeamos a TEXT con precisión.
        foreach (var prop in new[] { nameof(Envio.TarifaBase), nameof(Envio.Descuento), nameof(Envio.TarifaFinal), nameof(Envio.PesoKg) })
        {
            modelBuilder.Entity<Envio>().Property(prop).HasConversion<double>();
        }
    }
}
