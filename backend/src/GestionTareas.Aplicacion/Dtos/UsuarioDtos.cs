using System.ComponentModel.DataAnnotations;

namespace GestionTareas.Aplicacion.Dtos;

public class CrearUsuarioDto
{
    /// <example>Diana Torres</example>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede pasar de 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <example>diana@correo.com</example>
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede pasar de 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    /// <summary>Mínimo 6 caracteres.</summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
    public string Contrasena { get; set; } = string.Empty;
}

// Para editar un usuario solo se cambian nombre y correo.
public class ActualizarUsuarioDto
{
    /// <example>Diana Torres</example>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede pasar de 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <example>diana@correo.com</example>
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede pasar de 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}
