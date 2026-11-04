using System.Text.Json;
using GestionTareas.Dominio.Enumeraciones;
using GestionTareas.Dominio.Excepciones;

namespace GestionTareas.Dominio.Entidades;

public class Tarea
{
    public int Id { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public int UsuarioId { get; private set; }
    public EstadoTarea Estado { get; private set; }
    public PrioridadTarea Prioridad { get; private set; }
    public DateTime? FechaEstimada { get; private set; }

    // Las etiquetas se guardan en un solo texto separado por comas. Ejemplo: "api,urgente".
    public string? Etiquetas { get; private set; }

    // Texto JSON con datos libres de la tarea. En la BD es la columna NVARCHAR(MAX).
    public string? Metadatos { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public Usuario? Usuario { get; private set; }

    // Constructor vacío que necesita EF Core.
    private Tarea() { }

    public Tarea(string titulo, string? descripcion, int usuarioId, PrioridadTarea prioridad,
                 DateTime? fechaEstimada, string? etiquetas, string? metadatos)
    {
        Actualizar(titulo, descripcion, usuarioId, prioridad, fechaEstimada, etiquetas, metadatos);

        // Toda tarea nueva arranca pendiente.
        Estado = EstadoTarea.Pending;
        FechaCreacion = DateTime.UtcNow;
    }

    // Modifica los datos de la tarea (todo menos el estado, que tiene su propio método).
    public void Actualizar(string titulo, string? descripcion, int usuarioId, PrioridadTarea prioridad,
                           DateTime? fechaEstimada, string? etiquetas, string? metadatos)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ReglaNegocioException("El título de la tarea es obligatorio.");

        if (usuarioId <= 0)
            throw new ReglaNegocioException("La tarea debe tener un usuario asignado.");

        ValidarMetadatos(metadatos);

        Titulo = titulo.Trim();
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        UsuarioId = usuarioId;
        Prioridad = prioridad;
        FechaEstimada = fechaEstimada;
        Etiquetas = string.IsNullOrWhiteSpace(etiquetas) ? null : etiquetas;
        Metadatos = string.IsNullOrWhiteSpace(metadatos) ? null : metadatos;
    }

    // Regla de los estados:
    //   Pending    -> InProgress
    //   InProgress -> Done o volver a Pending
    //   Done       -> InProgress (reabrir)
    // Lo único prohibido es pasar de Pending a Done, o "cambiar" al mismo estado.
    public void CambiarEstado(EstadoTarea nuevoEstado)
    {
        if (nuevoEstado == Estado)
            throw new ReglaNegocioException($"La tarea ya se encuentra en estado {Estado}.");

        if (Estado == EstadoTarea.Pending && nuevoEstado == EstadoTarea.Done)
            throw new ReglaNegocioException("No se puede pasar una tarea de Pending a Done directamente. Primero debe estar en InProgress.");

        Estado = nuevoEstado;
    }

    // Revisa que el texto sea un JSON válido. En la BD hay un CHECK con ISJSON que hace lo mismo.
    private static void ValidarMetadatos(string? metadatos)
    {
        if (string.IsNullOrWhiteSpace(metadatos)) return;

        try
        {
            JsonDocument.Parse(metadatos);
        }
        catch (JsonException)
        {
            throw new ReglaNegocioException("Los metadatos no son un JSON válido.");
        }
    }
}
