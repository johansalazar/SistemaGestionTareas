import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { UsuarioApi } from '../../core/servicios/usuario-api';
import { mensajeDeError } from '../../core/mensaje-error';

// Formulario para crear o editar un usuario.
// Ruta /usuarios/nuevo = crear. Ruta /usuarios/:id/editar = editar.
@Component({
  selector: 'app-usuario-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './usuario-form.html',
})
export class UsuarioFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private api = inject(UsuarioApi);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  // Id del usuario que se edita. Si es null, se está creando uno nuevo.
  id: number | null = null;

  error = '';

  form = this.fb.nonNullable.group({
    nombre: ['', [Validators.required, Validators.maxLength(100)]],
    correo: ['', [Validators.required, Validators.email]],
    contrasena: ['', [Validators.required, Validators.minLength(6)]],
  });

  ngOnInit() {
    const idRuta = this.route.snapshot.paramMap.get('id');
    if (!idRuta) return;

    // Modo edición: no se pide contraseña y se cargan los datos actuales del usuario.
    this.id = Number(idRuta);
    this.form.controls.contrasena.disable();

    this.api.obtener(this.id).subscribe({
      next: (usuario) => this.form.patchValue({ nombre: usuario.nombre, correo: usuario.correo }),
      error: (e) => (this.error = mensajeDeError(e)),
    });
  }

  guardar() {
    this.error = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { nombre, correo, contrasena } = this.form.getRawValue();

    const peticion = this.id
      ? this.api.actualizar(this.id, { nombre, correo })
      : this.api.crear({ nombre, correo, contrasena });

    peticion.subscribe({
      next: () => {
        const mensaje = this.id ? 'Usuario actualizado.' : 'Usuario creado.';
        this.router.navigate(['/usuarios'], { state: { mensaje } });
      },
      error: (e) => (this.error = mensajeDeError(e)),
    });
  }
}
