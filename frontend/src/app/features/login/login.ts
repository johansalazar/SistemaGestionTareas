import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Auth } from '../../core/servicios/auth';
import { UsuarioApi } from '../../core/servicios/usuario-api';
import { mensajeDeError } from '../../core/mensaje-error';

// Pantalla para iniciar sesión o crear una cuenta nueva.
@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private auth = inject(Auth);
  private usuarioApi = inject(UsuarioApi);
  private router = inject(Router);

  modoRegistro = false;
  error = '';

  form = this.fb.nonNullable.group({
    nombre: [''],
    correo: ['', [Validators.required, Validators.email]],
    contrasena: ['', [Validators.required, Validators.minLength(6)]],
  });

  ngOnInit() {
    // Si ya hay sesión iniciada, no tiene sentido mostrar el login.
    if (this.auth.estaLogueado()) this.router.navigate(['/tareas']);
  }

  // Cambia entre "iniciar sesión" y "crear cuenta". El nombre solo es obligatorio al crear cuenta.
  cambiarModo() {
    this.modoRegistro = !this.modoRegistro;
    this.error = '';

    const nombre = this.form.controls.nombre;
    nombre.setValidators(this.modoRegistro ? [Validators.required] : []);
    nombre.updateValueAndValidity();
  }

  enviar() {
    this.error = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { nombre, correo, contrasena } = this.form.getRawValue();

    if (!this.modoRegistro) {
      this.auth.login(correo, contrasena).subscribe({
        next: () => this.router.navigate(['/tareas']),
        error: (e) => (this.error = mensajeDeError(e)),
      });
      return;
    }

    // Al crear la cuenta se inicia sesión de una vez con los mismos datos.
    this.usuarioApi.crear({ nombre, correo, contrasena }).subscribe({
      next: () => this.auth.login(correo, contrasena).subscribe(() => this.router.navigate(['/tareas'])),
      error: (e) => (this.error = mensajeDeError(e)),
    });
  }
}
