using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestionTareas.Aplicacion.Puertos.Salida;
using GestionTareas.Dominio.Entidades;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GestionTareas.Infraestructura.Seguridad;

// Valores que se leen de la sección "Jwt" del appsettings.json.
public class JwtOpciones
{
    public string Clave { get; set; } = string.Empty;
    public string Emisor { get; set; } = string.Empty;
    public string Audiencia { get; set; } = string.Empty;
    public int MinutosExpiracion { get; set; } = 480;
}

// Emite el token JWT firmado con la clave configurada.
public class GeneradorJwt : ITokenGenerador
{
    private readonly JwtOpciones _opciones;

    public GeneradorJwt(IOptions<JwtOpciones> opciones)
    {
        _opciones = opciones.Value;
    }

    public (string Token, DateTime Expira) Generar(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
            new Claim(JwtRegisteredClaimNames.Name, usuario.Nombre)
        };

        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Clave));
        var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddMinutes(_opciones.MinutosExpiracion);

        var token = new JwtSecurityToken(
            issuer: _opciones.Emisor,
            audience: _opciones.Audiencia,
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
