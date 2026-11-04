using GestionTareas.Aplicacion.Dtos;

namespace GestionTareas.Aplicacion.Puertos.Entrada;

public interface IAuthServicio
{
    Task<SesionDto> IniciarSesionAsync(LoginDto dto);
}
