using GestionTareas.Dominio.Entidades;

namespace GestionTareas.Aplicacion.Puertos.Salida;

// Puerto de salida para el manejo de contraseñas.
public interface IHasherContrasena
{
    string Hashear(string contrasena);
    bool Verificar(string contrasena, string hash);
}

// Puerto de salida para emitir el token de sesión.
public interface ITokenGenerador
{
    (string Token, DateTime Expira) Generar(Usuario usuario);
}
