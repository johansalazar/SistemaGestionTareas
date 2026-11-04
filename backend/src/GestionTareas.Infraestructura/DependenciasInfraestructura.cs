using GestionTareas.Aplicacion.Puertos.Salida;
using GestionTareas.Infraestructura.Persistencia;
using GestionTareas.Infraestructura.Repositorios;
using GestionTareas.Infraestructura.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionTareas.Infraestructura;

public static class DependenciasInfraestructura
{
    // Registra el DbContext (SQL Server), los repositorios y los servicios de seguridad
    // que implementan los puertos de salida.
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection servicios, IConfiguration configuracion)
    {
        var cadenaConexion = configuracion.GetConnectionString("GestionTareas")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'GestionTareas' en appsettings.json.");

        servicios.AddDbContext<GestionTareasDbContext>(opciones =>
            opciones.UseSqlServer(cadenaConexion));

        servicios.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        servicios.AddScoped<ITareaRepositorio, TareaRepositorio>();

        servicios.Configure<JwtOpciones>(configuracion.GetSection("Jwt"));
        servicios.AddSingleton<IHasherContrasena, HasherContrasena>();
        servicios.AddScoped<ITokenGenerador, GeneradorJwt>();
        return servicios;
    }
}
