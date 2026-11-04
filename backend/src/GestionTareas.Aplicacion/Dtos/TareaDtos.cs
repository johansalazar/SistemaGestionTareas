using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using GestionTareas.Dominio.Enumeraciones;

namespace GestionTareas.Aplicacion.Dtos;

public class CrearTareaDto
{
    /// <example>Preparar la demo</example>
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(150, ErrorMessage = "El título no puede pasar de 150 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    /// <example>Revisar que todo funcione antes de mostrarlo</example>
    [StringLength(500, ErrorMessage = "La descripción no puede pasar de 500 caracteres.")]
    public string? Descripcion { get; set; }

    /// <summary>Id del usuario al que se asigna la tarea.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Debe asignar un usuario a la tarea.")]
    public int UsuarioId { get; set; }

    /// <summary>Baja, Media o Alta.</summary>
    public PrioridadTarea Prioridad { get; set; } = PrioridadTarea.Media;

    /// <example>2026-12-01</example>
    public DateTime? FechaEstimada { get; set; }

    /// <summary>Lista de etiquetas. Ejemplo: ["api", "urgente"].</summary>
    public List<string>? Etiquetas { get; set; }

    /// <summary>Objeto JSON libre. Ejemplo: { "origen": "correo", "cliente": "ACME" }.</summary>
    public JsonElement? Metadatos { get; set; }
}

// Para editar una tarea se envían los mismos datos que al crearla. El estado se cambia por su propio endpoint.
public class ActualizarTareaDto : CrearTareaDto { }

// Cambia un solo campo dentro del JSON de metadatos. Si el valor no se envía (null), la clave se elimina.
public class ActualizarMetadatoDto
{
    /// <summary>Nombre del campo dentro del JSON.</summary>
    /// <example>cliente</example>
    [Required(ErrorMessage = "La clave es obligatoria.")]
    [RegularExpression("^[A-Za-z0-9_]{1,50}$", ErrorMessage = "La clave solo puede tener letras, números y guion bajo.")]
    public string Clave { get; set; } = string.Empty;

    /// <summary>Nuevo valor. Si no se envía, la clave se elimina.</summary>
    /// <example>ACME</example>
    public string? Valor { get; set; }
}

public class CambiarEstadoDto
{
    /// <summary>Pending, InProgress o Done.</summary>
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public EstadoTarea? Estado { get; set; }
}

public class TareaDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public EstadoTarea Estado { get; set; }
    public PrioridadTarea Prioridad { get; set; }
    public DateTime? FechaEstimada { get; set; }
    public List<string> Etiquetas { get; set; } = new();
    public JsonElement? Metadatos { get; set; }
    public DateTime FechaCreacion { get; set; }
}

// Filtros y paginación que acepta el GET /api/tasks por query string.
public class FiltroTareasDto : PaginacionDto
{
    public int? UsuarioId { get; set; }
    public EstadoTarea? Estado { get; set; }
    public PrioridadTarea? Prioridad { get; set; }

    /// <summary>Filtra por etiqueta.</summary>
    public string? Etiqueta { get; set; }

    /// <summary>Nombre del campo del JSON por el que se filtra. Se usa junto con metadatoValor.</summary>
    /// <example>origen</example>
    public string? MetadatoClave { get; set; }

    /// <summary>Valor que debe tener ese campo.</summary>
    /// <example>correo</example>
    public string? MetadatoValor { get; set; }
}
