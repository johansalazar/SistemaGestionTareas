using GestionTareas.Aplicacion.Dtos;
using GestionTareas.Aplicacion.Puertos.Entrada;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionTareas.Api.Controllers;

/// <summary>Gestión de tareas. Requiere token.</summary>
[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITareaServicio _servicio;

    public TasksController(ITareaServicio servicio)
    {
        _servicio = servicio;
    }

    /// <summary>Lista las tareas, paginadas y de la más reciente a la más antigua.</summary>
    /// <remarks>Todos los filtros son opcionales. Ejemplo: ?estado=Pending&amp;usuarioId=1&amp;metadatoClave=origen&amp;metadatoValor=correo</remarks>
    /// <response code="200">Página de tareas.</response>
    [HttpGet]
    public async Task<ActionResult<PaginaDto<TareaDto>>> Listar([FromQuery] FiltroTareasDto filtro)
    {
        return Ok(await _servicio.ListarAsync(filtro));
    }

    /// <summary>Devuelve una tarea por su id.</summary>
    /// <param name="id">Id de la tarea.</param>
    /// <response code="200">Tarea encontrada.</response>
    /// <response code="404">La tarea no existe.</response>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TareaDto>> Obtener(int id)
    {
        return Ok(await _servicio.ObtenerAsync(id));
    }

    /// <summary>Crea una tarea en estado Pending.</summary>
    /// <response code="201">Tarea creada.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="404">El usuario asignado no existe.</response>
    [HttpPost]
    public async Task<ActionResult<TareaDto>> Crear(CrearTareaDto dto)
    {
        var creada = await _servicio.CrearAsync(dto);
        return Created($"/api/tasks/{creada.Id}", creada);
    }

    /// <summary>Edita los datos de una tarea (no cambia su estado).</summary>
    /// <param name="id">Id de la tarea.</param>
    /// <response code="200">Tarea actualizada.</response>
    /// <response code="404">La tarea o el usuario no existen.</response>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TareaDto>> Actualizar(int id, ActualizarTareaDto dto)
    {
        return Ok(await _servicio.ActualizarAsync(id, dto));
    }

    /// <summary>Cambia el estado de una tarea.</summary>
    /// <remarks>Estados: Pending, InProgress, Done. No se puede pasar de Pending a Done directamente.</remarks>
    /// <param name="id">Id de la tarea.</param>
    /// <response code="200">Estado cambiado.</response>
    /// <response code="400">Cambio de estado no permitido.</response>
    /// <response code="404">La tarea no existe.</response>
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<TareaDto>> CambiarEstado(int id, CambiarEstadoDto dto)
    {
        return Ok(await _servicio.CambiarEstadoAsync(id, dto));
    }

    /// <summary>Cambia un solo campo del JSON de metadatos (usa JSON_MODIFY de SQL Server).</summary>
    /// <remarks>Si no se envía "valor", la clave se elimina.</remarks>
    /// <param name="id">Id de la tarea.</param>
    /// <response code="200">Metadato actualizado.</response>
    /// <response code="404">La tarea no existe.</response>
    [HttpPatch("{id:int}/metadatos")]
    public async Task<ActionResult<TareaDto>> ActualizarMetadato(int id, ActualizarMetadatoDto dto)
    {
        return Ok(await _servicio.ActualizarMetadatoAsync(id, dto));
    }

    /// <summary>Elimina una tarea.</summary>
    /// <param name="id">Id de la tarea.</param>
    /// <response code="204">Tarea eliminada.</response>
    /// <response code="404">La tarea no existe.</response>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _servicio.EliminarAsync(id);
        return NoContent();
    }
}
