using GestionTareas.Dominio.Excepciones;
using Microsoft.AspNetCore.Mvc;

namespace GestionTareas.Api.Middleware;

// Atrapa las excepciones que salen de los servicios y las convierte en una respuesta JSON con el código HTTP que toca.
// Así los controladores no necesitan try/catch.
public class ManejoErroresMiddleware
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ManejoErroresMiddleware> _logger;

    public ManejoErroresMiddleware(RequestDelegate siguiente, ILogger<ManejoErroresMiddleware> logger)
    {
        _siguiente = siguiente;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (ReglaNegocioException ex)
        {
            await Responder(contexto, StatusCodes.Status400BadRequest, "Regla de negocio", ex.Message);
        }
        catch (NoEncontradoException ex)
        {
            await Responder(contexto, StatusCodes.Status404NotFound, "No encontrado", ex.Message);
        }
        catch (ConflictoException ex)
        {
            await Responder(contexto, StatusCodes.Status409Conflict, "Conflicto", ex.Message);
        }
        catch (NoAutorizadoException ex)
        {
            await Responder(contexto, StatusCodes.Status401Unauthorized, "No autorizado", ex.Message);
        }
        catch (Exception ex)
        {
            // Cualquier otra cosa es un error nuestro: se registra completo pero al cliente no se le muestra el detalle.
            _logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}", contexto.Request.Method, contexto.Request.Path);
            await Responder(contexto, StatusCodes.Status500InternalServerError, "Error interno",
                "Ocurrió un error inesperado. Intente de nuevo más tarde.");
        }
    }

    private static Task Responder(HttpContext contexto, int codigo, string titulo, string detalle)
    {
        contexto.Response.StatusCode = codigo;
        var problema = new ProblemDetails { Status = codigo, Title = titulo, Detail = detalle };
        return contexto.Response.WriteAsJsonAsync(problema);
    }
}
