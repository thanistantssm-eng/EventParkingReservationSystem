import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-ui-state',
  standalone: true,
  template: `
    @if (type === 'loading') {
      <div class="skeleton" aria-label="Loading"><i></i><i></i><i></i><i></i></div>
    } @else {
      <div class="state" [class.error]="type === 'error'">
        <span>{{ type === 'error' ? '!' : '◇' }}</span>
        <h3>{{ title }}</h3>
        <p>{{ message }}</p>
        @if (actionLabel) {
          <button type="button" (click)="action.emit()">{{ actionLabel }}</button>
        }
      </div>
    }
  `,
  styles: [
    `
      :host {
        display: block;
      }
      .skeleton {
        display: grid;
        gap: 10px;
        padding: 16px;
      }
      .skeleton i {
        height: 54px;
        border-radius: 12px;
        background: linear-gradient(90deg, #f0f2f6 20%, #fafbfc 50%, #f0f2f6 80%);
        background-size: 200% 100%;
        animation: shimmer 1.25s infinite;
      }
      .state {
        padding: 42px 20px;
        text-align: center;
        color: #697184;
      }
      .state > span {
        width: 52px;
        height: 52px;
        margin: auto;
        display: grid;
        place-items: center;
        border-radius: 15px;
        background: #f1edff;
        color: #6b38df;
        font-size: 22px;
      }
      .state.error > span {
        background: #ffeded;
        color: #d84b4b;
      }
      .state h3 {
        margin: 12px 0 5px;
        color: #252b3d;
      }
      .state p {
        margin: 0 auto 14px;
        max-width: 430px;
        font-size: 10px;
      }
      .state button {
        padding: 9px 14px;
        border: 0;
        border-radius: 9px;
        background: #6436dc;
        color: #fff;
        font-size: 9px;
        font-weight: 800;
      }
      @keyframes shimmer {
        to {
          background-position: -200% 0;
        }
      }
    `,
  ],
})
export class UiState {
  @Input() type: 'loading' | 'empty' | 'error' = 'empty';
  @Input() title = 'Nothing here yet';
  @Input() message = 'There is no data to display.';
  @Input() actionLabel = '';
  @Output() readonly action = new EventEmitter<void>();
}
