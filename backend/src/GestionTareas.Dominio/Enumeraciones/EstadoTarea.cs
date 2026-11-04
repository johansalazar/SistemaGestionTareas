namespace GestionTareas.Dominio.Enumeraciones;

// Los nombres se dejan en inglés porque así los pide el documento de la prueba
// (Pending, InProgress, Done) y es lo que viaja en el JSON de la API.
public enum EstadoTarea
{
    Pending,
    InProgress,
    Done
}
