using GestionTareas.Aplicacion.Puertos.Salida;
using GestionTareas.Dominio.Entidades;
using GestionTareas.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace GestionTareas.Infraestructura.Repositorios;

// Adaptador de salida: implementa el puerto IUsuarioRepositorio.
public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly GestionTareasDbContext _contexto;

    public UsuarioRepositorio(GestionTareasDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task AgregarAsync(Usuario usuario)
    {
        _contexto.Usuarios.Add(usuario);
        await _contexto.SaveChangesAsync();
    }

    // Cuenta el total y trae solo la página pedida. AsNoTracking porque solo se lee para mostrar.
    public async Task<(List<Usuario> Items, int Total)> ListarAsync(int pagina, int tamano)
    {
        var consulta = _contexto.Usuarios.AsNoTracking();

        var total = await consulta.CountAsync();
        var items = await consulta
            .OrderBy(u => u.Nombre).ThenBy(u => u.Id)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }

    public Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        return _contexto.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    }

    public Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        return _contexto.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Correo == correo);
    }

    public Task<bool> ExisteAsync(int id)
    {
        return _contexto.Usuarios.AnyAsync(u => u.Id == id);
    }

    public Task<bool> ExisteCorreoAsync(string correo, int? excluirId = null)
    {
        return _contexto.Usuarios.AnyAsync(u => u.Correo == correo && u.Id != excluirId);
    }

    public Task<bool> TieneTareasAsync(int id)
    {
        return _contexto.Tareas.AnyAsync(t => t.UsuarioId == id);
    }

    public async Task EliminarAsync(Usuario usuario)
    {
        _contexto.Usuarios.Remove(usuario);
        await _contexto.SaveChangesAsync();
    }

    public Task GuardarCambiosAsync()
    {
        return _contexto.SaveChangesAsync();
    }
}
