import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { PRIORIDADES, Tarea, Usuario } from '../../core/modelos/modelos';
import { TareaApi } from '../../core/servicios/tarea-api';
import { UsuarioApi } from '../../core/servicios/usuario-api';
import { mensajeDeError } from '../../core/mensaje-error';

// Formulario para crear o editar una tarea.
// Ruta /tareas/nueva = crear. Ruta /tareas/:id/editar = editar.
@Component({
  selector: 'app-tarea-form',
  imports: [ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './tarea-form.html',
})
export class TareaFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private tareaApi = inject(TareaApi);
  private usuarioApi = inject(UsuarioApi);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  prioridades = PRIORIDADES;
  usuarios: Usuario[] = [];

  // Id de la tarea que se edita. Si es null, se está creando una nueva.
  id: number | null = null;

  error = '';
  mensaje = '';

  // Campos para cambiar una sola clave del JSON de metadatos (solo al editar).
  claveMetadato = '';
  valorMetadato = '';

  form = this.fb.group({
    titulo: ['', [Validators.required, Validators.maxLength(150)]],
    descripcion: [''],
    usuarioId: this.fb.control<number | null>(null, Validators.required),
    prioridad: ['Media'],
    fechaEstimada: [''],
    etiquetas: [''],
    metadatos: [''],
  });

  ngOnInit() {
    // Los usuarios se piden para el selector.
    this.usuarioApi.listar(1, 100).subscribe({
      next: (resultado) => (this.usuarios = resultado.items),
      error: (e) => (this.error = mensajeDeError(e)),
    });

    // Si la ruta trae un id, es edición: se cargan los datos actuales de la tarea.
    const idRuta = this.route.snapshot.paramMap.get('id');
    if (idRuta) {
      this.id = Number(idRuta);

      this.tareaApi.obtener(this.id).subscribe({
        next: (tarea) => this.cargarEnFormulario(tarea),
        error: (e) => (this.error = mensajeDeError(e)),
      });
    }
  }

  // Pasa los datos de la tarea al formulario.
  private cargarEnFormulario(tarea: Tarea) {
    this.form.patchValue({
      titulo: tarea.titulo,
      descripcion: tarea.descripcion ?? '',
      usuarioId: tarea.usuarioId,
      prioridad: tarea.prioridad,
      // El input de fecha necesita el formato yyyy-MM-dd.
      fechaEstimada: tarea.fechaEstimada ? tarea.fechaEstimada.substring(0, 10) : '',
      etiquetas: tarea.etiquetas.join(', '),
      metadatos: tarea.metadatos ? JSON.stringify(tarea.metadatos) : '',
    });
  }

  guardar() {
    this.error = '';
    this.mensaje = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();

    // Los metadatos se escriben como JSON en un cuadro de texto; se convierten a objeto antes de enviar.
    let metadatos = null;
    if (v.metadatos && v.metadatos.trim() !== '') {
      try {
        metadatos = JSON.parse(v.metadatos);
      } catch {
        this.error = 'Los metadatos no son un JSON válido. Ejemplo: {"origen": "correo"}';
        return;
      }
    }

    // Las etiquetas se escriben separadas por coma y se envían como lista.
    const etiquetas = (v.etiquetas ?? '')
      .split(',')
      .map((e) => e.trim())
      .filter((e) => e !== '');

    const datos = {
      titulo: v.titulo!,
      descripcion: v.descripcion || null,
      usuarioId: v.usuarioId!,
      prioridad: v.prioridad!,
      fechaEstimada: v.fechaEstimada || null,
      etiquetas,
      metadatos,
    };

    const peticion = this.id ? this.tareaApi.actualizar(this.id, datos) : this.tareaApi.crear(datos);

    peticion.subscribe({
      next: () => {
        const mensaje = this.id ? 'Tarea actualizada.' : 'Tarea creada.';
        this.router.navigate(['/tareas'], { state: { mensaje } });
      },
      error: (e) => (this.error = mensajeDeError(e)),
    });
  }

  // Cambia una sola clave del JSON usando el endpoint PATCH. Si el valor queda vacío, la clave se elimina.
  guardarMetadato() {
    if (!this.id || this.claveMetadato.trim() === '') return;

    this.error = '';
    this.mensaje = '';

    const valor = this.valorMetadato === '' ? null : this.valorMetadato;

    this.tareaApi.cambiarMetadato(this.id, this.claveMetadato.trim(), valor).subscribe({
      next: (tarea) => {
        this.mensaje = 'Campo de metadatos actualizado.';
        this.form.patchValue({ metadatos: tarea.metadatos ? JSON.stringify(tarea.metadatos) : '' });
        this.claveMetadato = '';
        this.valorMetadato = '';
      },
      error: (e) => (this.error = mensajeDeError(e)),
    });
  }
}
