using GestionTareas.Aplicacion.Dtos;

namespace GestionTareas.Aplicacion.Puertos.Entrada;

// Puerto de entrada para los casos de uso de tareas.
public interface ITareaServicio
{
    Task<TareaDto> CrearAsync(CrearTareaDto dto);
    Task<PaginaDto<TareaDto>> ListarAsync(FiltroTareasDto filtro);
    Task<TareaDto> ObtenerAsync(int id);
    Task<TareaDto> ActualizarAsync(int id, ActualizarTareaDto dto);
    Task<TareaDto> CambiarEstadoAsync(int id, CambiarEstadoDto dto);
    Task<TareaDto> ActualizarMetadatoAsync(int id, ActualizarMetadatoDto dto);
    Task EliminarAsync(int id);
}
