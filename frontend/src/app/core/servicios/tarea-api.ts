import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from '../config';
import { EstadoTarea, Pagina, Tarea } from '../modelos/modelos';

// Datos que se envían al crear o editar una tarea.
export interface DatosTarea {
  titulo: string;
  descripcion: string | null;
  usuarioId: number;
  prioridad: string;
  fechaEstimada: string | null;
  etiquetas: string[];
  metadatos: object | null;
}

@Injectable({ providedIn: 'root' })
export class TareaApi {
  private http = inject(HttpClient);

  // GET /api/tasks. Solo se envían los filtros que tengan valor.
  listar(estado: string, usuarioId: string, pagina: number) {
    let params = new HttpParams().set('pagina', pagina).set('tamano', 10);
    if (estado) params = params.set('estado', estado);
    if (usuarioId) params = params.set('usuarioId', usuarioId);

    return this.http.get<Pagina<Tarea>>(`${API_URL}/tasks`, { params });
  }

  // GET /api/tasks/{id}
  obtener(id: number) {
    return this.http.get<Tarea>(`${API_URL}/tasks/${id}`);
  }

  // POST /api/tasks
  crear(datos: DatosTarea) {
    return this.http.post<Tarea>(`${API_URL}/tasks`, datos);
  }

  // PUT /api/tasks/{id}
  actualizar(id: number, datos: DatosTarea) {
    return this.http.put<Tarea>(`${API_URL}/tasks/${id}`, datos);
  }

  // PUT /api/tasks/{id}/status
  cambiarEstado(id: number, estado: EstadoTarea) {
    return this.http.put<Tarea>(`${API_URL}/tasks/${id}/status`, { estado });
  }

  // PATCH /api/tasks/{id}/metadatos: cambia una sola clave del JSON (sin valor, la elimina).
  cambiarMetadato(id: number, clave: string, valor: string | null) {
    return this.http.patch<Tarea>(`${API_URL}/tasks/${id}/metadatos`, { clave, valor });
  }

  // DELETE /api/tasks/{id}
  eliminar(id: number) {
    return this.http.delete<void>(`${API_URL}/tasks/${id}`);
  }
}
