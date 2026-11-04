using GestionTareas.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionTareas.Infraestructura.Persistencia.Configuraciones;

public class TareaConfiguracion : IEntityTypeConfiguration<Tarea>
{
    public void Configure(EntityTypeBuilder<Tarea> builder)
    {
        // El CHECK con ISJSON hace que SQL Server rechace cualquier metadato que no sea JSON válido,
        // aunque alguien inserte directo en la tabla sin pasar por la API.
        builder.ToTable("Tasks", tabla =>
            tabla.HasCheckConstraint("CK_Tasks_Metadatos_EsJson", "[Metadatos] IS NULL OR ISJSON([Metadatos]) = 1"));

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Titulo).IsRequired().HasMaxLength(150);
        builder.Property(t => t.Descripcion).HasMaxLength(500);

        // Los enums se guardan como texto ("Pending", "Alta"...) para que se entienda al leer la tabla.
        builder.Property(t => t.Estado).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.Prioridad).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(t => t.Etiquetas).HasMaxLength(500);

        // Columna NVARCHAR(MAX) con los metadatos libres en formato JSON.
        builder.Property(t => t.Metadatos).HasColumnType("nvarchar(max)");

        builder.Property(t => t.FechaCreacion).IsRequired();

        // Una tarea siempre tiene usuario. Si se borra el usuario no se borran sus tareas (Restrict).
        builder.HasOne(t => t.Usuario)
            .WithMany(u => u.Tareas)
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice para la consulta principal: tareas de un usuario, filtradas por estado y ordenadas por fecha.
        builder.HasIndex(t => new { t.UsuarioId, t.Estado, t.FechaCreacion })
            .HasDatabaseName("IX_Tasks_Usuario_Estado_Fecha");
    }
}
