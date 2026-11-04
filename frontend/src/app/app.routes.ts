import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { LoginComponent } from './features/login/login';
import { TareasListaComponent } from './features/tareas/tareas-lista';
import { TareaFormComponent } from './features/tareas/tarea-form';
import { UsuariosListaComponent } from './features/usuarios/usuarios-lista';
import { UsuarioFormComponent } from './features/usuarios/usuario-form';

// Cada pantalla tiene su ruta. Todo menos el login exige sesión iniciada.
// Crear y editar usan el mismo formulario: si la ruta trae :id, el formulario edita.
export const routes: Routes = [
  { path: 'login', component: LoginComponent },

  { path: 'tareas', component: TareasListaComponent, canActivate: [authGuard] },
  { path: 'tareas/nueva', component: TareaFormComponent, canActivate: [authGuard] },
  { path: 'tareas/:id/editar', component: TareaFormComponent, canActivate: [authGuard] },

  { path: 'usuarios', component: UsuariosListaComponent, canActivate: [authGuard] },
  { path: 'usuarios/nuevo', component: UsuarioFormComponent, canActivate: [authGuard] },
  { path: 'usuarios/:id/editar', component: UsuarioFormComponent, canActivate: [authGuard] },

  // La raíz y cualquier ruta que no exista llevan a tareas.
  { path: '', redirectTo: 'tareas', pathMatch: 'full' },
  { path: '**', redirectTo: 'tareas' },
];
