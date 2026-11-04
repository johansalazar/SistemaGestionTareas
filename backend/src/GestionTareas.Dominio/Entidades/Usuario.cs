using GestionTareas.Dominio.Excepciones;

namespace GestionTareas.Dominio.Entidades;

public class Usuario
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Correo { get; private set; } = string.Empty;

    // Nunca se guarda la contraseña en claro, solo el resultado del hash.
    public string ContrasenaHash { get; private set; } = string.Empty;

    public DateTime FechaCreacion { get; private set; }

    public ICollection<Tarea> Tareas { get; private set; } = new List<Tarea>();

    // Constructor vacío que necesita EF Core para poder materializar la entidad desde la BD.
    private Usuario() { }

    public Usuario(string nombre, string correo, string contrasenaHash)
    {
        if (string.IsNullOrWhiteSpace(contrasenaHash))
            throw new ReglaNegocioException("El usuario debe tener contraseña.");

        Actualizar(nombre, correo);
        ContrasenaHash = contrasenaHash;
        FechaCreacion = DateTime.UtcNow;
    }

    // Cambia nombre y correo validando ambos. La contraseña no se toca desde aquí.
    public void Actualizar(string nombre, string correo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaNegocioException("El nombre del usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(correo) || !correo.Contains('@'))
            throw new ReglaNegocioException("El correo del usuario no es válido.");

        Nombre = nombre.Trim();
        // El correo se guarda en minúsculas para que la validación de duplicados no dependa de mayúsculas.
        Correo = correo.Trim().ToLowerInvariant();
    }
}
