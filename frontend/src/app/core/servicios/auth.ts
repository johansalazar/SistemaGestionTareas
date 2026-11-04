import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { tap } from 'rxjs';
import { API_URL } from '../config';
import { Sesion } from '../modelos/modelos';

// Maneja el inicio y cierre de sesión. El token y el nombre se guardan en localStorage
// para que la sesión siga abierta si se recarga la página.
@Injectable({ providedIn: 'root' })
export class Auth {
  private http = inject(HttpClient);

  login(correo: string, contrasena: string) {
    return this.http.post<Sesion>(`${API_URL}/auth/login`, { correo, contrasena }).pipe(
      tap((sesion) => {
        localStorage.setItem('token', sesion.token);
        localStorage.setItem('nombre', sesion.usuario.nombre);
        localStorage.setItem('usuarioId', String(sesion.usuario.id));
      }),
    );
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('nombre');
    localStorage.removeItem('usuarioId');
  }

  estaLogueado(): boolean {
    return localStorage.getItem('token') !== null;
  }

  // Id del usuario con la sesión iniciada (null si la sesión es anterior a este dato).
  usuarioId(): number | null {
    const id = localStorage.getItem('usuarioId');
    return id ? Number(id) : null;
  }

  nombre(): string {
    return localStorage.getItem('nombre') ?? '';
  }
}
