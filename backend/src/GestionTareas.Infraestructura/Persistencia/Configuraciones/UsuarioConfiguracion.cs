using GestionTareas.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionTareas.Infraestructura.Persistencia.Configuraciones;

public class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        // La tabla se llama Users como pide el documento de la prueba.
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Correo).IsRequired().HasMaxLength(150);
        builder.Property(u => u.ContrasenaHash).IsRequired().HasMaxLength(200);
        builder.Property(u => u.FechaCreacion).IsRequired();

        // Dos usuarios no pueden compartir correo.
        builder.HasIndex(u => u.Correo).IsUnique().HasDatabaseName("UX_Users_Correo");
    }
}
