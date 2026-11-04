using GestionTareas.Aplicacion.Dtos;

namespace GestionTareas.Aplicacion.Puertos.Entrada;

// Puerto de entrada: lo que la API (u otro adaptador) puede pedirle a la aplicación sobre usuarios.
public interface IUsuarioServicio
{
    Task<UsuarioDto> CrearAsync(CrearUsuarioDto dto);
    Task<PaginaDto<UsuarioDto>> ListarAsync(PaginacionDto paginacion);
    Task<UsuarioDto> ObtenerAsync(int id);
    Task<UsuarioDto> ActualizarAsync(int id, ActualizarUsuarioDto dto);
    // usuarioActualId es el usuario que hizo la petición (el que tiene la sesión iniciada).
    Task EliminarAsync(int id, int usuarioActualId);
}
