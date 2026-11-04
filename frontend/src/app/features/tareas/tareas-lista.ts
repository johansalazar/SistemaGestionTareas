import { Component, inject, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ESTADOS, EstadoTarea, SIGUIENTES_ESTADOS, Tarea, Usuario } from '../../core/modelos/modelos';
import { TareaApi } from '../../core/servicios/tarea-api';
import { UsuarioApi } from '../../core/servicios/usuario-api';
import { mensajeDeError } from '../../core/mensaje-error';
import { ModalConfirmacionComponent } from '../../shared/modal-confirmacion';

// Pantalla con la lista de tareas: filtros, paginación, cambio de estado y eliminar.
// Crear y editar se hacen en el formulario (otra pantalla).
@Component({
  selector: 'app-tareas-lista',
  imports: [FormsModule, DatePipe, RouterLink, ModalConfirmacionComponent],
  templateUrl: './tareas-lista.html',
  styleUrl: './tareas-lista.css',
})
export class TareasListaComponent implements OnInit {
  private tareaApi = inject(TareaApi);
  private usuarioApi = inject(UsuarioApi);

  estados = ESTADOS;

  tareas: Tarea[] = [];
  usuarios: Usuario[] = [];

  // Filtros de la lista.
  filtroEstado = '';
  filtroUsuario = '';

  pagina = 1;
  totalPaginas = 1;

  // Tarea que se quiere eliminar. Mientras tenga valor se muestra el modal de confirmación.
  tareaAEliminar: Tarea | null = null;

  error = '';

  // Mensaje que manda el formulario al volver aquí (por ejemplo "Tarea creada.").
  mensaje = history.state?.mensaje ?? '';

  ngOnInit() {
    // Los usuarios se piden para el filtro por usuario.
    this.usuarioApi.listar(1, 100).subscribe({
      next: (resultado) => (this.usuarios = resultado.items),
      error: (e) => (this.error = mensajeDeError(e)),
    });

    this.cargar();
  }

  cargar() {
    this.tareaApi.listar(this.filtroEstado, this.filtroUsuario, this.pagina).subscribe({
      next: (resultado) => {
        // Si se borró la última tarea de la última página, se vuelve a la anterior.
        if (resultado.items.length === 0 && this.pagina > 1) {
          this.pagina--;
          this.cargar();
          return;
        }
        this.tareas = resultado.items;
        this.totalPaginas = resultado.totalPaginas || 1;
      },
      error: (e) => (this.error = mensajeDeError(e)),
    });
  }

  // Al cambiar un filtro se vuelve a la primera página.
  filtrar() {
    this.pagina = 1;
    this.cargar();
  }

  cambiarPagina(pagina: number) {
    this.pagina = pagina;
    this.cargar();
  }

  siguientesEstados(tarea: Tarea): EstadoTarea[] {
    return SIGUIENTES_ESTADOS[tarea.estado];
  }

  cambiarEstado(tarea: Tarea, estado: EstadoTarea) {
    this.error = '';
    this.mensaje = '';

    this.tareaApi.cambiarEstado(tarea.id, estado).subscribe({
      next: () => this.cargar(),
      error: (e) => (this.error = mensajeDeError(e)),
    });
  }

  // Se ejecuta cuando el usuario confirma en el modal.
  eliminar() {
    if (!this.tareaAEliminar) return;

    const id = this.tareaAEliminar.id;
    this.tareaAEliminar = null;
    this.error = '';
    this.mensaje = '';

    this.tareaApi.eliminar(id).subscribe({
      next: () => {
        this.mensaje = 'Tarea eliminada.';
        this.cargar();
      },
      error: (e) => (this.error = mensajeDeError(e)),
    });
  }

  verMetadatos(tarea: Tarea): string {
    return tarea.metadatos ? JSON.stringify(tarea.metadatos) : '';
  }
}
