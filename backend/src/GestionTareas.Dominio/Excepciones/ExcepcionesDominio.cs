namespace GestionTareas.Dominio.Excepciones;

// Se lanza cuando se rompe una regla de negocio (título vacío, cambio de estado inválido, etc.).
// La API la traduce a un 400.
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}

// Se lanza cuando se busca algo por id y no existe. La API la traduce a un 404.
public class NoEncontradoException : Exception
{
    public NoEncontradoException(string mensaje) : base(mensaje) { }
}

// Se lanza cuando la operación choca con datos existentes (correo repetido, usuario con tareas...). La API la traduce a un 409.
public class ConflictoException : Exception
{
    public ConflictoException(string mensaje) : base(mensaje) { }
}

// Se lanza cuando las credenciales del login no son correctas. La API la traduce a un 401.
public class NoAutorizadoException : Exception
{
    public NoAutorizadoException(string mensaje) : base(mensaje) { }
}
