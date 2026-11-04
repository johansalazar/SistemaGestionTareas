using GestionTareas.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace GestionTareas.Infraestructura.Persistencia;

public class GestionTareasDbContext : DbContext
{
    public GestionTareasDbContext(DbContextOptions<GestionTareasDbContext> opciones) : base(opciones) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Tarea> Tareas => Set<Tarea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Toma todas las clases de configuración (IEntityTypeConfiguration) de este proyecto.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GestionTareasDbContext).Assembly);
    }
}
