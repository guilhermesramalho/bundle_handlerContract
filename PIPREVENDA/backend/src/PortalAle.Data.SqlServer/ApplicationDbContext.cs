using Microsoft.EntityFrameworkCore;

namespace PortalAle.Data.SqlServer;

/// <summary>
/// DbContext principal do domínio (schema "dbo"). O schema de staging da
/// integração SAP ("Stg") é isolado em PortalAle.Data.SapStaging — nunca
/// misturar com este contexto. Ver DT-003, DT-004.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");

        // Descobre automaticamente todas as classes IEntityTypeConfiguration<T>
        // criadas em Persistencia/Configuracoes/. Ver DT-004.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
