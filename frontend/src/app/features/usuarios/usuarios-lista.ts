import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Usuario } from '../../core/modelos/modelos';
import { Auth } from '../../core/servicios/auth';
import { UsuarioApi } from '../../core/servicios/usuario-api';
import { mensajeDeError } from '../../core/mensaje-error';
import { ModalConfirmacionComponent } from '../../shared/modal-confirmacion';

// Pantalla con la lista de usuarios (paginada). Crear y editar se hacen en el formulario.
@Component({
  selector: 'app-usuarios-lista',
  imports: [RouterLink, ModalConfirmacionComponent],
  templateUrl: './usuarios-lista.html',
  styleUrl: './usuarios-lista.css',
})
export class UsuariosListaComponent implements OnInit {
  private api = inject(UsuarioApi);
  private auth = inject(Auth);

  usuarios: Usuario[] = [];
  pagina = 1;
  totalPaginas = 1;

  // Usuario que se quiere eliminar. Mientras tenga valor se muestra el modal de confirmación.
  usuarioAEliminar: Usuario | null = null;

  error = '';

  // Mensaje que manda el formulario al volver aquí (por ejemplo "Usuario creado.").
  mensaje = history.state?.mensaje ?? '';

  ngOnInit() {
    this.cargar();
  }

  cargar() {
    this.api.listar(this.pagina).subscribe({
      next: (resultado) => {
        // Si se borró el último usuario de la última página, se vuelve a la anterior.
        if (resultado.items.length === 0 && this.pagina > 1) {
          this.pagina--;
          this.cargar();
          return;
        }
        this.usuarios = resultado.items;
        this.totalPaginas = resultado.totalPaginas || 1;
      },
      error: (e) => (this.error = mensajeDeError(e)),
    });
  }

  cambiarPagina(pagina: number) {
    this.pagina = pagina;
    this.cargar();
  }

  // Se ejecuta al pulsar "Eliminar". Si es el usuario logueado, solo se muestra el aviso y no se abre el modal.
  pedirEliminar(usuario: Usuario) {
    this.mensaje = '';

    if (usuario.id === this.auth.usuarioId()) {
      this.error = 'No puedes eliminar tu propio usuario mientras tienes la sesión iniciada.';
      return;
    }

    this.error = '';
    this.usuarioAEliminar = usuario;
  }

  // Se ejecuta cuando el usuario confirma en el modal.
  eliminar() {
    if (!this.usuarioAEliminar) return;

    const id = this.usuarioAEliminar.id;
    this.usuarioAEliminar = null;
    this.error = '';
    this.mensaje = '';

    this.api.eliminar(id).subscribe({
      next: () => {
        this.mensaje = 'Usuario eliminado.';
        this.cargar();
      },
      error: (e) => (this.error = mensajeDeError(e)),
    });
  }
}
