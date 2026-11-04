using System.Text.Json;
using GestionTareas.Aplicacion.Dtos;
using GestionTareas.Aplicacion.Puertos.Entrada;
using GestionTareas.Aplicacion.Puertos.Salida;
using GestionTareas.Dominio.Entidades;
using GestionTareas.Dominio.Excepciones;

namespace GestionTareas.Aplicacion.Servicios;

public class TareaServicio : ITareaServicio
{
    private readonly ITareaRepositorio _tareas;
    private readonly IUsuarioRepositorio _usuarios;

    public TareaServicio(ITareaRepositorio tareas, IUsuarioRepositorio usuarios)
    {
        _tareas = tareas;
        _usuarios = usuarios;
    }

    // Crea una tarea. Primero verifica que el usuario asignado exista y luego la entidad valida el resto.
    public async Task<TareaDto> CrearAsync(CrearTareaDto dto)
    {
        await ValidarUsuarioAsync(dto.UsuarioId);

        var tarea = new Tarea(dto.Titulo, dto.Descripcion, dto.UsuarioId, dto.Prioridad,
                              dto.FechaEstimada, EtiquetasATexto(dto.Etiquetas), MetadatosATexto(dto.Metadatos));

        await _tareas.AgregarAsync(tarea);

        // Se vuelve a consultar para traer también el nombre del usuario asignado.
        var creada = await _tareas.ObtenerPorIdAsync(tarea.Id);
        return Mapear(creada!);
    }

    // Lista una página de tareas aplicando los filtros que lleguen.
    public async Task<PaginaDto<TareaDto>> ListarAsync(FiltroTareasDto filtro)
    {
        var (tareas, total) = await _tareas.ListarAsync(filtro);

        return new PaginaDto<TareaDto>
        {
            Items = tareas.Select(Mapear).ToList(),
            Total = total,
            Pagina = filtro.Pagina,
            Tamano = filtro.Tamano
        };
    }

    // Devuelve una tarea por su id, o 404 si no existe.
    public async Task<TareaDto> ObtenerAsync(int id)
    {
        return Mapear(await ObtenerOFallarAsync(id));
    }

    // Edita los datos de una tarea (todo menos el estado). Si cambia el usuario, el nuevo debe existir.
    public async Task<TareaDto> ActualizarAsync(int id, ActualizarTareaDto dto)
    {
        var tarea = await ObtenerOFallarAsync(id);
        await ValidarUsuarioAsync(dto.UsuarioId);

        tarea.Actualizar(dto.Titulo, dto.Descripcion, dto.UsuarioId, dto.Prioridad,
                         dto.FechaEstimada, EtiquetasATexto(dto.Etiquetas), MetadatosATexto(dto.Metadatos));

        await _tareas.GuardarCambiosAsync();

        // Se consulta de nuevo para que venga el nombre del usuario por si se reasignó la tarea.
        var actualizada = await _tareas.ObtenerPorIdAsync(id);
        return Mapear(actualizada!);
    }

    // Cambia el estado de una tarea. La regla de qué cambios se permiten vive en la entidad.
    public async Task<TareaDto> CambiarEstadoAsync(int id, CambiarEstadoDto dto)
    {
        var tarea = await ObtenerOFallarAsync(id);

        tarea.CambiarEstado(dto.Estado!.Value);
        await _tareas.GuardarCambiosAsync();

        return Mapear(tarea);
    }

    // Cambia un solo campo del JSON de metadatos sin tocar el resto. El trabajo lo hace SQL Server con JSON_MODIFY.
    public async Task<TareaDto> ActualizarMetadatoAsync(int id, ActualizarMetadatoDto dto)
    {
        await ObtenerOFallarAsync(id);
        await _tareas.ActualizarMetadatoAsync(id, dto.Clave, dto.Valor);

        // El repositorio limpia lo que tenía en memoria, así que esta consulta ya trae el JSON actualizado.
        var actualizada = await ObtenerOFallarAsync(id);
        return Mapear(actualizada);
    }

    public async Task EliminarAsync(int id)
    {
        var tarea = await ObtenerOFallarAsync(id);
        await _tareas.EliminarAsync(tarea);
    }

    private async Task<Tarea> ObtenerOFallarAsync(int id)
    {
        return await _tareas.ObtenerPorIdAsync(id)
            ?? throw new NoEncontradoException($"No existe la tarea con id {id}.");
    }

    private async Task ValidarUsuarioAsync(int usuarioId)
    {
        if (!await _usuarios.ExisteAsync(usuarioId))
            throw new NoEncontradoException($"No existe el usuario con id {usuarioId}.");
    }

    // Une las etiquetas en un solo texto separado por comas, sin vacías ni repetidas.
    private static string? EtiquetasATexto(List<string>? etiquetas)
    {
        if (etiquetas == null) return null;

        var limpias = etiquetas
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim())
            .Distinct();

        return string.Join(",", limpias);
    }

    // Pasa los metadatos del DTO a texto JSON. Un null explícito en el body se trata como "sin metadatos".
    private static string? MetadatosATexto(JsonElement? metadatos)
    {
        if (metadatos == null || metadatos.Value.ValueKind == JsonValueKind.Null) return null;
        return metadatos.Value.GetRawText();
    }

    // Convierte la entidad al DTO. Las etiquetas pasan de texto a lista y los metadatos de texto a JSON real,
    // para que la API los devuelva como arreglo y objeto y no como strings.
    private static TareaDto Mapear(Tarea t)
    {
        JsonElement? metadatos = null;
        if (!string.IsNullOrWhiteSpace(t.Metadatos))
            metadatos = JsonDocument.Parse(t.Metadatos).RootElement.Clone();

        var etiquetas = string.IsNullOrWhiteSpace(t.Etiquetas)
            ? new List<string>()
            : t.Etiquetas.Split(',').ToList();

        return new TareaDto
        {
            Id = t.Id,
            Titulo = t.Titulo,
            Descripcion = t.Descripcion,
            UsuarioId = t.UsuarioId,
            UsuarioNombre = t.Usuario?.Nombre ?? string.Empty,
            Estado = t.Estado,
            Prioridad = t.Prioridad,
            FechaEstimada = t.FechaEstimada,
            Etiquetas = etiquetas,
            Metadatos = metadatos,
            FechaCreacion = t.FechaCreacion
        };
    }
}
