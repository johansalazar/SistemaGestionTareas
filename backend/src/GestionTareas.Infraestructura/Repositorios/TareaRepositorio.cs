using GestionTareas.Aplicacion.Dtos;
using GestionTareas.Aplicacion.Puertos.Salida;
using GestionTareas.Dominio.Entidades;
using GestionTareas.Dominio.Excepciones;
using GestionTareas.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace GestionTareas.Infraestructura.Repositorios;

// Adaptador de salida: implementa el puerto ITareaRepositorio con EF Core.
public class TareaRepositorio : ITareaRepositorio
{
    private readonly GestionTareasDbContext _contexto;

    public TareaRepositorio(GestionTareasDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task AgregarAsync(Tarea tarea)
    {
        _contexto.Tareas.Add(tarea);
        await _contexto.SaveChangesAsync();
    }

    // Trae la tarea con su usuario.
    public Task<Tarea?> ObtenerPorIdAsync(int id)
    {
        return _contexto.Tareas
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public Task GuardarCambiosAsync()
    {
        return _contexto.SaveChangesAsync();
    }

    public async Task EliminarAsync(Tarea tarea)
    {
        _contexto.Tareas.Remove(tarea);
        await _contexto.SaveChangesAsync();
    }

    // Arma la consulta sumando solo los filtros que vengan con valor, cuenta el total y trae la página pedida.
    public async Task<(List<Tarea> Items, int Total)> ListarAsync(FiltroTareasDto filtro)
    {
        IQueryable<Tarea> consulta = _contexto.Tareas;

        // Filtro por un campo dentro del JSON: se usa JSON_VALUE de SQL Server.
        // La consulta va con parámetros, así que no hay riesgo de inyección SQL.
        if (!string.IsNullOrWhiteSpace(filtro.MetadatoClave) && !string.IsNullOrWhiteSpace(filtro.MetadatoValor))
        {
            // La clave forma parte de la ruta JSON, por eso solo se aceptan letras, números y guion bajo.
            if (!filtro.MetadatoClave.All(c => char.IsLetterOrDigit(c) || c == '_'))
                throw new ReglaNegocioException("La clave del metadato solo puede tener letras, números y guion bajo.");

            var ruta = "$." + filtro.MetadatoClave.Trim();
            var valor = filtro.MetadatoValor.Trim();
            consulta = _contexto.Tareas.FromSqlInterpolated(
                $"SELECT * FROM Tasks WHERE JSON_VALUE(Metadatos, {ruta}) = {valor}");
        }

        consulta = consulta.Include(t => t.Usuario);

        if (filtro.UsuarioId.HasValue)
            consulta = consulta.Where(t => t.UsuarioId == filtro.UsuarioId.Value);

        if (filtro.Estado.HasValue)
            consulta = consulta.Where(t => t.Estado == filtro.Estado.Value);

        if (filtro.Prioridad.HasValue)
            consulta = consulta.Where(t => t.Prioridad == filtro.Prioridad.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Etiqueta))
            consulta = consulta.Where(t => t.Etiquetas != null && t.Etiquetas.Contains(filtro.Etiqueta.Trim()));

        var total = await consulta.CountAsync();

        // Las más recientes primero, y se corta la página pedida.
        var items = await consulta
            .OrderByDescending(t => t.FechaCreacion)
            .Skip((filtro.Pagina - 1) * filtro.Tamano)
            .Take(filtro.Tamano)
            .ToListAsync();

        return (items, total);
    }

    // Modifica un solo campo del JSON con JSON_MODIFY, sin reescribir el resto del documento.
    // Si Metadatos está vacío se parte de un objeto vacío "{}". Si el valor es null, JSON_MODIFY elimina la clave.
    public async Task ActualizarMetadatoAsync(int id, string clave, string? valor)
    {
        var ruta = "$." + clave;

        await _contexto.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Tasks SET Metadatos = JSON_MODIFY(ISNULL(Metadatos, N'{{}}'), {ruta}, {valor}) WHERE Id = {id}");

        // El UPDATE se hizo por fuera de EF; se limpia lo que EF tenía en memoria para que la próxima lectura vaya a la BD.
        _contexto.ChangeTracker.Clear();
    }
}
