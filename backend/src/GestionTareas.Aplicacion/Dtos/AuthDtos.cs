using System.ComponentModel.DataAnnotations;

namespace GestionTareas.Aplicacion.Dtos;

public class LoginDto
{
    /// <example>ana@correo.com</example>
    [Required(ErrorMessage = "El correo es obligatorio.")]
    public string Correo { get; set; } = string.Empty;

    /// <example>123456</example>
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Contrasena { get; set; } = string.Empty;
}

// Lo que recibe el cliente al iniciar sesión: el token JWT, cuándo vence y quién es el usuario.
public class SesionDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
    public UsuarioDto Usuario { get; set; } = new();
}
