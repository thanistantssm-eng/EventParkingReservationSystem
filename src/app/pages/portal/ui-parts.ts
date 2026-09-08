import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-stepper',
  standalone: true,
  template: `<div class="flow-stepper" aria-label="Booking progress">
    <span class="step-node done">✓<small>Event</small></span
    ><i></i
    ><span
      class="step-node"
      [class.active]="active === 'ticket'"
      [class.done]="active !== 'ticket'"
    >
      {{ active === 'ticket' ? '1' : '✓' }}<small>Tickets</small></span
    ><i></i
    ><span class="step-node" [class.active]="active === 'seat'" [class.done]="active === 'parking'">
      {{ active === 'parking' ? '✓' : '2' }}<small>Seats</small></span
    ><i></i
    ><span class="step-node" [class.active]="active === 'parking'">3<small>Parking</small></span
    ><i></i><span class="step-node">4<small>Summary</small></span>
  </div>`,
  styles: [
    `
      :host {
        display: block;
      }
      .flow-stepper {
        max-width: 720px;
        margin: 0 auto 36px;
        display: flex;
        align-items: center;
      }
      .flow-stepper .step-node {
        width: 34px;
        height: 34px;
        border-radius: 50%;
        border: 2px solid #e3e6ed;
        background: #fff;
        color: #9aa2b4;
        font-weight: 800;
        position: relative;
      }
      .flow-stepper .step-node small {
        position: absolute;
        top: 39px;
        left: 50%;
        transform: translateX(-50%);
        font-size: 11px;
        white-space: nowrap;
        color: #8a92a5;
      }
      .flow-stepper i {
        height: 2px;
        flex: 1;
        background: #e9ebf0;
      }
      .flow-stepper .step-node.active,
      .flow-stepper .step-node.done {
        border-color: #ff6b0b;
        background: #ff6b0b;
        color: #fff;
      }
      .flow-stepper .step-node.done + i {
        background: #ff6b0b;
      }
      .flow-stepper .step-node.active small,
      .flow-stepper .step-node.done small {
        color: #ff6b0b;
      }
    `,
  ],
})
export class Stepper {
  @Input() active = 'ticket';
}

@Component({
  selector: 'app-order-summary',
  standalone: true,
  template: `<aside class="order-panel">
    <h2>Order Summary</h2>
    <div class="order-event">
      <div class="event-photo p0"></div>
      <div><b>Summer Music Fest</b><small>24 May 2026 • 6:00 PM</small></div>
    </div>
    <div class="detail-list">
      <p>
        <span>Ticket</span><b>₹{{ basePrice }}</b>
      </p>
      <p>
        <span>Seat</span><b>{{ seat }}</b>
      </p>
      <p>
        <span>Parking</span><b>{{ parking ? '₹' + parking : '—' }}</b>
      </p>
      <p><span>Taxes & fees</span><b>₹106</b></p>
    </div>
    <div class="grand-total">
      <span>Total</span><b>₹{{ total }}</b>
    </div>
    <ng-content></ng-content><small class="centered">🔒 Secure, encrypted checkout</small>
  </aside>`,
})
export class OrderSummary {
  @Input() basePrice = 0;
  @Input() parking = 0;
  @Input() total = 0;
  @Input() seat = 'A2';
}
