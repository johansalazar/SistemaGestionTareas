using GestionTareas.Aplicacion.Dtos;
using GestionTareas.Aplicacion.Puertos.Entrada;
using GestionTareas.Aplicacion.Puertos.Salida;
using GestionTareas.Dominio.Excepciones;

namespace GestionTareas.Aplicacion.Servicios;

public class AuthServicio : IAuthServicio
{
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IHasherContrasena _hasher;
    private readonly ITokenGenerador _tokens;

    public AuthServicio(IUsuarioRepositorio usuarios, IHasherContrasena hasher, ITokenGenerador tokens)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _tokens = tokens;
    }

    // Valida correo y contraseña y devuelve el token. Si algo falla se responde siempre el mismo mensaje,
    // para no revelar si lo que no existe es el correo o la contraseña.
    public async Task<SesionDto> IniciarSesionAsync(LoginDto dto)
    {
        var usuario = await _usuarios.ObtenerPorCorreoAsync(dto.Correo.Trim().ToLowerInvariant());

        if (usuario == null || !_hasher.Verificar(dto.Contrasena, usuario.ContrasenaHash))
            throw new NoAutorizadoException("Correo o contraseña incorrectos.");

        var (token, expira) = _tokens.Generar(usuario);

        return new SesionDto
        {
            Token = token,
            Expira = expira,
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                FechaCreacion = usuario.FechaCreacion
            }
        };
    }
}
