import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { Auth } from './servicios/auth';

// Agrega el token a todas las peticiones para que la API sepa quién es el usuario.
// Si la API responde 401 estando logueado, el token venció: se cierra la sesión y se manda al login.
export const tokenInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(Auth);
  const router = inject(Router);
  const token = localStorage.getItem('token');

  if (token) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }

  return next(req).pipe(
    catchError((error) => {
      // El 401 del login es "contraseña incorrecta", no un token vencido, por eso se excluye.
      const esLogin = req.url.endsWith('/auth/login');

      if (error instanceof HttpErrorResponse && error.status === 401 && token && !esLogin) {
        auth.logout();
        router.navigate(['/login']);
      }

      return throwError(() => error);
    }),
  );
};
