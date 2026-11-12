import { Dialog, DialogRef } from '@angular/cdk/dialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  effect,
  inject,
  input,
  model,
  output,
  TemplateRef,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { Button, ButtonVariant } from '../button/button';

let nextId = 0;

// Diálogo de confirmación sobre CDK Dialog: atrapa el foco, se cierra con Esc y devuelve el foco al disparador.
// Se abre cuando "open" pasa a true; al cerrarse por cualquier vía, "open" vuelve a false.
@Component({
  selector: 'lt-confirm-dialog',
  imports: [Button],
  templateUrl: './confirm-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDialog {
  readonly open = model(false);
  readonly title = input.required<string>();
  readonly message = input.required<string>();
  readonly confirmLabel = input('Confirmar');
  readonly cancelLabel = input('Cancelar');
  readonly confirmVariant = input<ButtonVariant>('primary');
  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  protected readonly titleId = `lt-confirm-title-${nextId++}`;

  private readonly dialog = inject(Dialog);
  private readonly viewContainerRef = inject(ViewContainerRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly content = viewChild.required<TemplateRef<unknown>>('content');
  private dialogRef: DialogRef<unknown> | null = null;

  constructor() {
    effect(() => {
      if (this.open()) {
        this.openDialog();
      } else {
        this.dialogRef?.close();
      }
    });
  }

  protected confirm(): void {
    this.confirmed.emit();
    this.dialogRef?.close();
  }

  protected cancel(): void {
    this.cancelled.emit();
    this.dialogRef?.close();
  }

  private openDialog(): void {
    if (this.dialogRef) {
      return;
    }

    const ref = this.dialog.open(this.content(), {
      viewContainerRef: this.viewContainerRef,
      ariaLabelledBy: this.titleId,
      backdropClass: 'bg-primary/40',
      panelClass: [
        'mx-space-md',
        'w-full',
        'max-w-md',
        'rounded-xl',
        'bg-surface-container-lowest',
        'p-space-lg',
        'shadow-soft-md',
      ],
    });

    this.dialogRef = ref;
    ref.closed.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.dialogRef = null;
      this.open.set(false);
    });
  }
}
