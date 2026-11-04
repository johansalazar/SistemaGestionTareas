export type EstadoTarea = 'Pending' | 'InProgress' | 'Done';
export type PrioridadTarea = 'Baja' | 'Media' | 'Alta';

export const ESTADOS: EstadoTarea[] = ['Pending', 'InProgress', 'Done'];
export const PRIORIDADES: PrioridadTarea[] = ['Baja', 'Media', 'Alta'];

// A qué estados se puede pasar desde cada uno. Solo sirve para mostrar los botones correctos;
// la validación real la hace la API.
export const SIGUIENTES_ESTADOS: Record<EstadoTarea, EstadoTarea[]> = {
  Pending: ['InProgress'],
  InProgress: ['Pending', 'Done'],
  Done: ['InProgress'],
};

// Forma de las respuestas paginadas de la API.
export interface Pagina<T> {
  items: T[];
  total: number;
  pagina: number;
  tamano: number;
  totalPaginas: number;
}

export interface Usuario {
  id: number;
  nombre: string;
  correo: string;
  fechaCreacion: string;
}

export interface Sesion {
  token: string;
  expira: string;
  usuario: Usuario;
}

export interface Tarea {
  id: number;
  titulo: string;
  descripcion: string | null;
  usuarioId: number;
  usuarioNombre: string;
  estado: EstadoTarea;
  prioridad: PrioridadTarea;
  fechaEstimada: string | null;
  etiquetas: string[];
  metadatos: Record<string, unknown> | null;
  fechaCreacion: string;
}
