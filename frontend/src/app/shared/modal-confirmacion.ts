import { Component, EventEmitter, Input, Output } from '@angular/core';

// Ventana de confirmación. El componente que la usa decide cuándo mostrarla (con @if)
// y qué hacer cuando el usuario confirma o cancela.
@Component({
  selector: 'app-modal-confirmacion',
  template: `
    <div class="fondo" (click)="cancelar.emit()">
      <div class="ventana" (click)="$event.stopPropagation()">
        <h3>{{ titulo }}</h3>
        <p>{{ mensaje }}</p>

        <div class="botones">
          <button class="secundario" (click)="cancelar.emit()">Cancelar</button>
          <button class="peligro" (click)="confirmar.emit()">{{ textoConfirmar }}</button>
        </div>
      </div>
    </div>
  `,
  styles: `
    .fondo {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.45);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 100;
    }

    .ventana {
      background: #fff;
      border-radius: 6px;
      padding: 20px 24px;
      width: 100%;
      max-width: 400px;
    }

    h3 {
      margin: 0 0 8px;
    }

    p {
      margin: 0 0 20px;
      color: #444;
    }

    .botones {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
    }
  `,
})
export class ModalConfirmacionComponent {
  @Input() titulo = 'Confirmar';
  @Input() mensaje = '';
  @Input() textoConfirmar = 'Eliminar';

  @Output() confirmar = new EventEmitter<void>();
  @Output() cancelar = new EventEmitter<void>();
}
