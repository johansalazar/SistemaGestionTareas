import { HttpErrorResponse } from '@angular/common/http';

// Convierte el error HTTP en un texto para mostrarle al usuario.
// La API responde { detail } para reglas de negocio y { errors: { Campo: [mensajes] } } para validaciones.
export function mensajeDeError(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) return 'Ocurrió un error inesperado.';

  // status 0 significa que ni siquiera se pudo conectar con la API.
  if (error.status === 0) return 'No se pudo conectar con el servidor. Verifique que la API esté encendida.';

  if (error.error?.errors) {
    return Object.values(error.error.errors as Record<string, string[]>)
      .flat()
      .join(' ');
  }

  if (error.error?.detail) return error.error.detail;

  if (error.status === 401) return 'Su sesión venció. Salga e inicie sesión de nuevo.';

  return `Error ${error.status}: no se pudo completar la operación.`;
}
