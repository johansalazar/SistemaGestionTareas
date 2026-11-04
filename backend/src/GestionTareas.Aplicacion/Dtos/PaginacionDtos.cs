using System.ComponentModel.DataAnnotations;

namespace GestionTareas.Aplicacion.Dtos;

// Parámetros de paginación que llegan por query string (?pagina=1&tamano=10).
public class PaginacionDto
{
    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser 1 o mayor.")]
    public int Pagina { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
    public int Tamano { get; set; } = 10;
}

// Respuesta paginada: los elementos de la página pedida y los totales para armar el paginador.
public class PaginaDto<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Tamano { get; set; }
    public int TotalPaginas => (int)Math.Ceiling(Total / (double)Tamano);
}
