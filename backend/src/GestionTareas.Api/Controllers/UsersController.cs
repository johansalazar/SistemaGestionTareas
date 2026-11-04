using GestionTareas.Aplicacion.Dtos;
using GestionTareas.Aplicacion.Puertos.Entrada;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionTareas.Api.Controllers;

/// <summary>Gestión de usuarios. Todo requiere token excepto crear un usuario (registro).</summary>
[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUsuarioServicio _servicio;

    public UsersController(IUsuarioServicio servicio)
    {
        _servicio = servicio;
    }

    /// <summary>Lista los usuarios, paginados y ordenados por nombre.</summary>
    /// <response code="200">Página de usuarios.</response>
    [HttpGet]
    public async Task<ActionResult<PaginaDto<UsuarioDto>>> Listar([FromQuery] PaginacionDto paginacion)
    {
        return Ok(await _servicio.ListarAsync(paginacion));
    }

    /// <summary>Devuelve un usuario por su id.</summary>
    /// <param name="id">Id del usuario.</param>
    /// <response code="200">Usuario encontrado.</response>
    /// <response code="404">El usuario no existe.</response>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Obtener(int id)
    {
        return Ok(await _servicio.ObtenerAsync(id));
    }

    /// <summary>Crea un usuario. No requiere token.</summary>
    /// <response code="201">Usuario creado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="409">Ya existe un usuario con ese correo.</response>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<UsuarioDto>> Crear(CrearUsuarioDto dto)
    {
        var creado = await _servicio.CrearAsync(dto);
        return Created($"/api/users/{creado.Id}", creado);
    }

    /// <summary>Edita el nombre y el correo de un usuario.</summary>
    /// <param name="id">Id del usuario.</param>
    /// <response code="200">Usuario actualizado.</response>
    /// <response code="404">El usuario no existe.</response>
    /// <response code="409">El correo ya lo usa otro usuario.</response>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Actualizar(int id, ActualizarUsuarioDto dto)
    {
        return Ok(await _servicio.ActualizarAsync(id, dto));
    }

    /// <summary>Elimina un usuario.</summary>
    /// <param name="id">Id del usuario.</param>
    /// <response code="204">Usuario eliminado.</response>
    /// <response code="404">El usuario no existe.</response>
    /// <response code="409">Es el usuario con la sesión iniciada, o tiene tareas asignadas.</response>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        // El id del usuario logueado viene dentro del token (claim "sub").
        var usuarioActualId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? "0");

        await _servicio.EliminarAsync(id, usuarioActualId);
        return NoContent();
    }
}
