using GestionTareas.Aplicacion.Puertos.Entrada;
using GestionTareas.Aplicacion.Servicios;
using Microsoft.Extensions.DependencyInjection;

namespace GestionTareas.Aplicacion;

public static class DependenciasAplicacion
{
    // Registra los servicios de la capa de aplicación.
    public static IServiceCollection AgregarAplicacion(this IServiceCollection servicios)
    {
        servicios.AddScoped<IUsuarioServicio, UsuarioServicio>();
        servicios.AddScoped<ITareaServicio, TareaServicio>();
        servicios.AddScoped<IAuthServicio, AuthServicio>();
        return servicios;
    }
}
