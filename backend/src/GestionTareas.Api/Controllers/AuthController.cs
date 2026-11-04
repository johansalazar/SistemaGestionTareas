using GestionTareas.Aplicacion.Dtos;
using GestionTareas.Aplicacion.Puertos.Entrada;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionTareas.Api.Controllers;

/// <summary>Inicio de sesión.</summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthServicio _servicio;

    public AuthController(IAuthServicio servicio)
    {
        _servicio = servicio;
    }

    /// <summary>Inicia sesión y devuelve el token para usar en los demás endpoints.</summary>
    /// <remarks>Usuario de prueba: ana@correo.com / 123456 (después de ejecutar el script de datos de prueba).</remarks>
    /// <response code="200">Token generado.</response>
    /// <response code="401">Correo o contraseña incorrectos.</response>
    [HttpPost("login")]
    public async Task<ActionResult<SesionDto>> Login(LoginDto dto)
    {
        return Ok(await _servicio.IniciarSesionAsync(dto));
    }
}
