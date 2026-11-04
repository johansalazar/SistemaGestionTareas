import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from '../config';
import { Pagina, Usuario } from '../modelos/modelos';

@Injectable({ providedIn: 'root' })
export class UsuarioApi {
  private http = inject(HttpClient);

  // GET /api/users
  listar(pagina = 1, tamano = 10) {
    return this.http.get<Pagina<Usuario>>(`${API_URL}/users?pagina=${pagina}&tamano=${tamano}`);
  }

  // GET /api/users/{id}
  obtener(id: number) {
    return this.http.get<Usuario>(`${API_URL}/users/${id}`);
  }

  // POST /api/users (no pide token: sirve para registrarse)
  crear(usuario: { nombre: string; correo: string; contrasena: string }) {
    return this.http.post<Usuario>(`${API_URL}/users`, usuario);
  }

  // PUT /api/users/{id}
  actualizar(id: number, usuario: { nombre: string; correo: string }) {
    return this.http.put<Usuario>(`${API_URL}/users/${id}`, usuario);
  }

  // DELETE /api/users/{id}
  eliminar(id: number) {
    return this.http.delete<void>(`${API_URL}/users/${id}`);
  }
}
