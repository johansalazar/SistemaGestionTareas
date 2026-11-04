using GestionTareas.Dominio.Entidades;

namespace GestionTareas.Aplicacion.Puertos.Salida;

// Puerto de salida: lo que la aplicación necesita de la persistencia. Lo implementa Infraestructura.
public interface IUsuarioRepositorio
{
    Task AgregarAsync(Usuario usuario);

    // Devuelve solo la página pedida y el total de usuarios existentes.
    Task<(List<Usuario> Items, int Total)> ListarAsync(int pagina, int tamano);

    // Trae el usuario con tracking, listo para modificarse.
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);

    Task<bool> ExisteAsync(int id);

    // excluirId sirve al editar: el propio usuario no cuenta como duplicado de sí mismo.
    Task<bool> ExisteCorreoAsync(string correo, int? excluirId = null);

    Task<bool> TieneTareasAsync(int id);
    Task EliminarAsync(Usuario usuario);
    Task GuardarCambiosAsync();
}
