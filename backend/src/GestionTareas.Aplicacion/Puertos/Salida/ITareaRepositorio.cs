using GestionTareas.Aplicacion.Dtos;
using GestionTareas.Dominio.Entidades;

namespace GestionTareas.Aplicacion.Puertos.Salida;

public interface ITareaRepositorio
{
    Task AgregarAsync(Tarea tarea);
    Task<Tarea?> ObtenerPorIdAsync(int id);

    // Aplica filtros, orden y paginación. Devuelve la página y el total que cumple los filtros.
    Task<(List<Tarea> Items, int Total)> ListarAsync(FiltroTareasDto filtro);

    // Guarda los cambios hechos a una tarea que ya fue consultada.
    Task GuardarCambiosAsync();

    Task EliminarAsync(Tarea tarea);

    // Cambia un solo campo del JSON de metadatos directamente en SQL Server (JSON_MODIFY).
    // Si valor es null, la clave se elimina del JSON.
    Task ActualizarMetadatoAsync(int id, string clave, string? valor);
}
