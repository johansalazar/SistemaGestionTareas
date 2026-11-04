using GestionTareas.Aplicacion.Dtos;
using GestionTareas.Aplicacion.Puertos.Entrada;
using GestionTareas.Aplicacion.Puertos.Salida;
using GestionTareas.Dominio.Entidades;
using GestionTareas.Dominio.Excepciones;

namespace GestionTareas.Aplicacion.Servicios;

public class UsuarioServicio : IUsuarioServicio
{
    private readonly IUsuarioRepositorio _repositorio;
    private readonly IHasherContrasena _hasher;

    public UsuarioServicio(IUsuarioRepositorio repositorio, IHasherContrasena hasher)
    {
        _repositorio = repositorio;
        _hasher = hasher;
    }

    // Crea el usuario después de comprobar que el correo no lo tenga ya otro.
    // La contraseña se guarda hasheada. El formato del correo y del nombre lo valida la entidad.
    public async Task<UsuarioDto> CrearAsync(CrearUsuarioDto dto)
    {
        var usuario = new Usuario(dto.Nombre, dto.Correo, _hasher.Hashear(dto.Contrasena));

        if (await _repositorio.ExisteCorreoAsync(usuario.Correo))
            throw new ConflictoException($"Ya existe un usuario con el correo {usuario.Correo}.");

        await _repositorio.AgregarAsync(usuario);
        return Mapear(usuario);
    }

    // Devuelve una página de usuarios, ordenados por nombre desde el repositorio.
    public async Task<PaginaDto<UsuarioDto>> ListarAsync(PaginacionDto paginacion)
    {
        var (usuarios, total) = await _repositorio.ListarAsync(paginacion.Pagina, paginacion.Tamano);

        return new PaginaDto<UsuarioDto>
        {
            Items = usuarios.Select(Mapear).ToList(),
            Total = total,
            Pagina = paginacion.Pagina,
            Tamano = paginacion.Tamano
        };
    }

    // Devuelve un usuario por su id, o 404 si no existe.
    public async Task<UsuarioDto> ObtenerAsync(int id)
    {
        var usuario = await _repositorio.ObtenerPorIdAsync(id)
            ?? throw new NoEncontradoException($"No existe el usuario con id {id}.");

        return Mapear(usuario);
    }

    // Edita nombre y correo. El correo nuevo no puede pertenecer a otro usuario.
    public async Task<UsuarioDto> ActualizarAsync(int id, ActualizarUsuarioDto dto)
    {
        var usuario = await _repositorio.ObtenerPorIdAsync(id)
            ?? throw new NoEncontradoException($"No existe el usuario con id {id}.");

        usuario.Actualizar(dto.Nombre, dto.Correo);

        if (await _repositorio.ExisteCorreoAsync(usuario.Correo, excluirId: id))
            throw new ConflictoException($"Ya existe un usuario con el correo {usuario.Correo}.");

        await _repositorio.GuardarCambiosAsync();
        return Mapear(usuario);
    }

    // Elimina el usuario con estas reglas:
    //  - No se puede eliminar al usuario que tiene la sesión iniciada.
    //  - Solo se elimina si no tiene tareas; si las tiene hay que reasignarlas o borrarlas primero.
    public async Task EliminarAsync(int id, int usuarioActualId)
    {
        if (id == usuarioActualId)
            throw new ConflictoException("No puedes eliminar tu propio usuario mientras tienes la sesión iniciada.");

        var usuario = await _repositorio.ObtenerPorIdAsync(id)
            ?? throw new NoEncontradoException($"No existe el usuario con id {id}.");

        if (await _repositorio.TieneTareasAsync(id))
            throw new ConflictoException("No se puede eliminar un usuario que tiene tareas asignadas. Reasigne o elimine sus tareas primero.");

        await _repositorio.EliminarAsync(usuario);
    }

    private static UsuarioDto Mapear(Usuario u) => new()
    {
        Id = u.Id,
        Nombre = u.Nombre,
        Correo = u.Correo,
        FechaCreacion = u.FechaCreacion
    };
}
