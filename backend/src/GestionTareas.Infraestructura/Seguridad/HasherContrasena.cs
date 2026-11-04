using GestionTareas.Aplicacion.Puertos.Salida;
using Microsoft.AspNetCore.Identity;

namespace GestionTareas.Infraestructura.Seguridad;

// Hash de contraseñas con el PasswordHasher que ya trae .NET (usa PBKDF2 con sal aleatoria).
// No se programa el algoritmo a mano: solo se usa esta clase.
public class HasherContrasena : IHasherContrasena
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hashear(string contrasena)
    {
        return _hasher.HashPassword(new object(), contrasena);
    }

    public bool Verificar(string contrasena, string hash)
    {
        var resultado = _hasher.VerifyHashedPassword(new object(), hash, contrasena);
        return resultado != PasswordVerificationResult.Failed;
    }
}
