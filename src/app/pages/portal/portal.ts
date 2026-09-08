import { CommonModule } from '@angular/common';
import { Component, ViewEncapsulation, computed, effect, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter } from 'rxjs';
import { OrderSummary, Stepper } from './ui-parts';

type EventItem = {
  id: string;
  title: string;
  venue: string;
  date: string;
  time: string;
  category: string;
  price: number;
  image: number;
  format: string;
  status: string;
};

const bookingState = {
  ticket: 'premium',
  seat: 'A2',
  parking: 'B27',
  payment: 'card',
  promoApplied: false,
};

@Component({
  selector: 'app-portal',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, Stepper, OrderSummary],
  templateUrl: './portal.html',
  styleUrl: './portal.css',
  encapsulation: ViewEncapsulation.None,
})
export class Portal {
  protected readonly sidebarOpen = signal(false);
  protected readonly profileOpen = signal(false);
  protected readonly bookingModal = signal(false);
  protected readonly toast = signal('');
  protected readonly view = signal('dashboard');
  protected readonly activeEventId = signal('summer');
  protected readonly detailTab = signal<'about' | 'venue' | 'gallery'>('about');
  protected readonly bookingTab = signal<'upcoming' | 'completed' | 'cancelled'>('upcoming');
  protected readonly parkingFilter = signal<'all' | 'active' | 'upcoming'>('all');
  protected readonly notificationFilter = signal<
    'all' | 'unread' | 'bookings' | 'payments' | 'parking'
  >('all');
  protected readonly settingsTab = signal<'general' | 'security' | 'notifications' | 'privacy'>(
    'general',
  );
  protected readonly query = signal('');
  protected readonly category = signal('All');
  protected readonly favoriteIds = signal(new Set<string>(['summer']));
  protected readonly selectedTicket = signal(bookingState.ticket);
  protected readonly selectedSeat = signal(bookingState.seat);
  protected readonly selectedParking = signal(bookingState.parking);
  protected readonly selectedPayment = signal(bookingState.payment);
  protected readonly promo = signal('');
  protected readonly promoApplied = signal(bookingState.promoApplied);
  protected readonly cardNumber = signal('');
  protected readonly expiry = signal('');
  protected readonly cvv = signal('');
  protected readonly notifications = signal<Record<string, boolean>>({
    booking: true,
    payment: true,
    event: true,
    parking: true,
    ticket: true,
    offers: false,
  });

  protected readonly events: EventItem[] = [
    {
      id: 'summer',
      title: 'Summer Music Fest',
      venue: 'Jawaharlal Nehru Stadium',
      date: '24 May 2026',
      time: '6:00 PM',
      category: 'Concert',
      price: 799,
      image: 0,
      format: 'Seat Based',
      status: 'Available',
    },
    {
      id: 'food',
      title: 'Food Carnival 2026',
      venue: 'Island Grounds, Chennai',
      date: '10 Jun 2026',
      time: '4:00 PM',
      category: 'Festival',
      price: 399,
      image: 1,
      format: 'General Entry',
      status: 'Selling Fast',
    },
    {
      id: 'tech',
      title: 'Tech Conference',
      venue: 'Chennai Trade Centre',
      date: '15 Jul 2026',
      time: '9:30 AM',
      category: 'Conference',
      price: 999,
      image: 2,
      format: 'Seat Based',
      status: 'Available',
    },
    {
      id: 'league',
      title: 'Football League Final',
      venue: 'Marina Arena',
      date: '28 Aug 2026',
      time: '7:30 PM',
      category: 'Sports',
      price: 1299,
      image: 3,
      format: 'Seat Based',
      status: 'Available',
    },
  ];

  protected readonly visibleEvents = computed(() => {
    const q = this.query().trim().toLowerCase();
    const category = this.category();
    return this.events.filter(
      (item) =>
        (!q || `${item.title} ${item.venue} ${item.category}`.toLowerCase().includes(q)) &&
        (category === 'All' || item.category === category),
    );
  });
  protected readonly activeEvent = computed(
    () => this.events.find((event) => event.id === this.activeEventId()) ?? this.events[0],
  );

  protected readonly basePrice = computed(() =>
    this.selectedTicket() === 'vip'
      ? 1999
      : this.selectedTicket() === 'premium'
        ? 1499
        : this.selectedTicket() === 'couple'
          ? 2499
          : 799,
  );
  protected readonly parkingPrice = computed(() => (this.selectedParking() ? 300 : 0));
  protected readonly discount = computed(() => (this.promoApplied() ? 300 : 0));
  protected readonly total = computed(
    () => this.basePrice() + this.parkingPrice() + 106 - this.discount(),
  );

  constructor(private readonly router: Router) {
    try {
      const saved = JSON.parse(localStorage.getItem('eventora-booking') || '{}');
      const savedFavorites = JSON.parse(localStorage.getItem('eventora-favorites') || '[]');
      if (Array.isArray(savedFavorites))
        this.favoriteIds.set(
          new Set(savedFavorites.filter((id): id is string => typeof id === 'string')),
        );
      if (saved.ticket) this.selectedTicket.set(saved.ticket);
      if (saved.seat) this.selectedSeat.set(saved.seat);
      if (typeof saved.parking === 'string') this.selectedParking.set(saved.parking);
      if (saved.payment) this.selectedPayment.set(saved.payment);
      if (typeof saved.promoApplied === 'boolean') this.promoApplied.set(saved.promoApplied);
    } catch {
      /* keep demo defaults */
    }
    this.syncView(this.router.url);
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => {
        this.syncView(event.urlAfterRedirects);
        this.sidebarOpen.set(false);
        window.scrollTo({ top: 0, behavior: 'smooth' });
      });
    effect(() => {
      try {
        localStorage.setItem('eventora-favorites', JSON.stringify([...this.favoriteIds()]));
      } catch {
        /* preview environments may block storage */
      }
    });
    effect(() => {
      bookingState.ticket = this.selectedTicket();
      bookingState.seat = this.selectedSeat();
      bookingState.parking = this.selectedParking();
      bookingState.payment = this.selectedPayment();
      bookingState.promoApplied = this.promoApplied();
      try {
        localStorage.setItem('eventora-booking', JSON.stringify(bookingState));
      } catch {
        /* preview environments may block storage */
      }
    });
  }

  private syncView(url: string): void {
    const clean = url.split('?')[0].replace(/^\//, '');
    if (clean.startsWith('admin')) this.view.set('admin');
    else if (clean.startsWith('organizer')) this.view.set('organizer');
    else if (clean.startsWith('customer/booking/tickets/')) {
      this.activeEventId.set(clean.split('/').at(-1) || 'summer');
      this.view.set('tickets');
    } else if (clean.startsWith('customer/booking/seats/')) {
      this.activeEventId.set(clean.split('/').at(-1) || 'summer');
      this.view.set('seats');
    } else if (clean.startsWith('customer/booking/parking/')) {
      this.activeEventId.set(clean.split('/').at(-1) || 'summer');
      this.view.set('parking-select');
    } else if (clean.startsWith('customer/booking/summary/')) {
      this.activeEventId.set(clean.split('/').at(-1) || 'summer');
      this.view.set('checkout');
    } else if (clean.startsWith('customer/booking/payment/')) {
      this.activeEventId.set(clean.split('/').at(-1) || 'summer');
      this.view.set('payment');
    } else if (clean.startsWith('customer/booking/success/')) this.view.set('confirmed');
    else if (clean.startsWith('customer/booking/ticket/')) this.view.set('ticket');
    else if (clean === 'customer/dashboard') this.view.set('dashboard');
    else if (clean === 'customer/events') this.view.set('events');
    else if (clean.startsWith('customer/events/')) {
      this.activeEventId.set(clean.split('/').at(-1) || 'summer');
      this.view.set('event-detail');
    } else if (clean === 'customer/bookings') this.view.set('bookings');
    else if (clean.startsWith('customer/bookings/')) this.view.set('booking-detail');
    else if (clean === 'customer/parking') this.view.set('my-parking');
    else if (clean === 'customer/payments' || clean === 'customer/payment-history')
      this.view.set('payments');
    else if (clean === 'customer/payments/receipt') this.view.set('receipt');
    else if (clean === 'customer/notifications') this.view.set('notifications');
    else if (clean === 'customer/favorites') this.view.set('favorites');
    else if (clean === 'customer/profile') this.view.set('profile');
    else if (clean === 'customer/support') this.view.set('support');
    else if (clean === 'customer/settings') this.view.set('settings');
    else if (clean.includes('events/summer/tickets')) this.view.set('tickets');
    else if (clean.includes('events/summer/seats')) this.view.set('seats');
    else if (clean.includes('events/summer/parking')) this.view.set('parking-select');
    else if (clean.includes('events/summer')) this.view.set('event-detail');
    else if (clean === 'app/events') this.view.set('events');
    else if (clean === 'app/checkout') this.view.set('checkout');
    else if (clean === 'app/payment') this.view.set('payment');
    else if (clean === 'app/confirmed') this.view.set('confirmed');
    else if (clean === 'app/bookings/summer') this.view.set('booking-detail');
    else if (clean === 'app/bookings') this.view.set('bookings');
    else if (clean === 'app/ticket/summer') this.view.set('ticket');
    else if (clean === 'app/parking') this.view.set('my-parking');
    else if (clean === 'app/payments/receipt') this.view.set('receipt');
    else if (clean === 'app/payments') this.view.set('payments');
    else if (clean === 'app/notifications') this.view.set('notifications');
    else this.view.set('dashboard');
  }

  protected go(path: string): void {
    const legacyRoutes: Record<string, string> = {
      '/app/dashboard': '/customer/dashboard',
      '/app/events': '/customer/events',
      '/app/events/summer': '/customer/events/summer',
      '/app/events/summer/tickets': '/customer/booking/tickets/summer',
      '/app/events/summer/seats': '/customer/booking/seats/summer',
      '/app/events/summer/parking': '/customer/booking/parking/summer',
      '/app/checkout': '/customer/booking/summary/summer',
      '/app/payment': '/customer/booking/payment/summer',
      '/app/confirmed': '/customer/booking/success/EP-05124',
      '/app/bookings': '/customer/bookings',
      '/app/bookings/summer': '/customer/bookings/EP-05124',
      '/app/ticket/summer': '/customer/booking/ticket/EP-05124',
      '/app/parking': '/customer/parking',
      '/app/payments': '/customer/payments',
      '/app/payments/receipt': '/customer/payments/receipt',
      '/app/notifications': '/customer/notifications',
    };
    void this.router.navigateByUrl(legacyRoutes[path] ?? path);
  }
  protected signOut(): void {
    try {
      sessionStorage.removeItem('eventora-role');
    } catch {
      /* storage may be blocked in preview */
    }
    this.profileOpen.set(false);
    this.go('/login');
  }
  protected setQuery(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }
  protected toggleFavorite(id: string): void {
    const next = new Set(this.favoriteIds());
    next.has(id) ? next.delete(id) : next.add(id);
    this.favoriteIds.set(next);
    this.flash(next.has(id) ? 'Added to favorites' : 'Removed from favorites');
  }
  protected chooseTicket(id: string): void {
    bookingState.ticket = id;
    this.selectedTicket.set(id);
  }
  protected chooseSeat(seat: string): void {
    bookingState.seat = seat;
    this.selectedSeat.set(seat);
  }
  protected chooseParking(slot: string): void {
    const next = this.selectedParking() === slot ? '' : slot;
    bookingState.parking = next;
    this.selectedParking.set(next);
  }
  protected clearParking(): void {
    bookingState.parking = '';
    this.selectedParking.set('');
  }
  protected choosePayment(method: string): void {
    bookingState.payment = method;
    this.selectedPayment.set(method);
  }
  protected applyPromo(): void {
    const valid = this.promo().trim().toUpperCase() === 'EVENT300';
    this.promoApplied.set(valid);
    bookingState.promoApplied = valid;
    this.flash(valid ? 'Promo code applied — ₹300 saved' : 'Try code EVENT300');
  }
  protected toggleNotification(key: string): void {
    this.notifications.update((items) => ({ ...items, [key]: !items[key] }));
  }
  protected pay(): void {
    if (this.selectedPayment() === 'card' && this.cardNumber().replace(/\s/g, '').length < 12) {
      this.flash('Enter a valid card number');
      return;
    }
    this.go('/customer/booking/success/EP-05124');
  }
  protected confirmCancel(): void {
    this.bookingModal.set(false);
    this.flash('Cancellation request submitted');
  }
  protected download(label = 'Ticket'): void {
    const body = `${label}\nEventora\nSummer Music Fest\nSeat: ${this.selectedSeat()}\nParking: ${this.selectedParking() || 'Not selected'}\nTotal: ₹${this.total()}`;
    const url = URL.createObjectURL(new Blob([body], { type: 'text/plain;charset=utf-8' }));
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = `${label.toLowerCase().replace(/\s+/g, '-')}.txt`;
    anchor.click();
    URL.revokeObjectURL(url);
    this.flash(`${label} downloaded`);
  }
  protected shareTicket(): void {
    const text = `Summer Music Fest ticket — seat ${this.selectedSeat()}`;
    if (navigator.share) {
      void navigator
        .share({ title: 'Eventora Ticket', text })
        .catch(() => this.flash('Sharing cancelled'));
      return;
    }
    void navigator.clipboard?.writeText(text);
    this.flash('Ticket details copied');
  }
  protected flash(message: string): void {
    this.toast.set(message);
    window.setTimeout(() => this.toast.set(''), 2600);
  }
  protected spritePosition(index: number): string {
    return `${index * 33.333}% center`;
  }
}
