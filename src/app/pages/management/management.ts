import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter } from 'rxjs';

type Role = 'organizer' | 'admin';
type RowStatus =
  'Approved' | 'Pending' | 'Draft' | 'Rejected' | 'Confirmed' | 'Paid' | 'Active' | 'Inactive';

type EventRow = {
  id: string;
  title: string;
  venue: string;
  date: string;
  type: string;
  status: RowStatus;
  bookings: number;
  revenue: string;
  image: string;
};

@Component({
  selector: 'app-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './management.html',
  styleUrl: './management.css',
})
export class Management {
  protected readonly role = signal<Role>('organizer');
  protected readonly view = signal('dashboard');
  protected readonly toast = signal('');
  protected readonly sidebarOpen = signal(false);
  protected readonly query = signal('');
  protected readonly statusFilter = signal('All');
  protected readonly activeWizardStep = signal(1);
  protected readonly showModal = signal(false);
  protected readonly seatCells = Array.from({ length: 64 });
  protected readonly miniSeatCells = Array.from({ length: 36 });
  protected readonly parkingCells = Array.from({ length: 48 });
  protected readonly modalMode = signal<'approve' | 'reject' | 'delete'>('approve');
  protected readonly selectedName = signal('');
  protected readonly selectedSeatIndex = signal<number | null>(null);
  protected readonly selectedParkingIndex = signal<number | null>(null);
  protected readonly selectedWizardType = signal<'seat' | 'general'>('seat');
  protected readonly setupMode = signal<'seat' | 'general'>('seat');
  protected readonly notificationTab = signal('All');
  protected readonly settingsSection = signal('General');

  protected readonly organizerEvents: EventRow[] = [
    {
      id: 'summer',
      title: 'Summer Music Fest',
      venue: 'SoundWave Arena',
      date: '24 May 2026 • 6:00 PM',
      type: 'Seat Based',
      status: 'Approved',
      bookings: 256,
      revenue: '₹4,86,400',
      image: '/assets/concert-hero.webp',
    },
    {
      id: 'food',
      title: 'Food Carnival 2026',
      venue: 'Island Grounds',
      date: '10 Jun 2026 • 4:00 PM',
      type: 'Non-Seat Based',
      status: 'Pending',
      bookings: 184,
      revenue: '₹2,10,900',
      image: '/assets/event-grid.webp',
    },
    {
      id: 'tech',
      title: 'Tech Conference 2026',
      venue: 'Chennai Trade Centre',
      date: '18 Jul 2026 • 9:30 AM',
      type: 'Seat Based',
      status: 'Draft',
      bookings: 0,
      revenue: '₹0',
      image: '/assets/eventora-event-sprite.png',
    },
    {
      id: 'league',
      title: 'Football League Final',
      venue: 'Marina Arena',
      date: '06 Aug 2026 • 7:00 PM',
      type: 'Seat Based',
      status: 'Approved',
      bookings: 402,
      revenue: '₹6,42,500',
      image: '/assets/event-grid.webp',
    },
  ];

  protected readonly filteredEvents = computed(() => {
    const q = this.query().toLowerCase().trim();
    const s = this.statusFilter();
    return this.organizerEvents.filter(
      (e) =>
        (!q || `${e.title} ${e.venue} ${e.type}`.toLowerCase().includes(q)) &&
        (s === 'All' || e.status === s),
    );
  });

  protected readonly adminNav = [
    ['Dashboard', '/admin/dashboard', '⌂'],
    ['Properties', '/admin/properties', '▥'],
    ['Organizers', '/admin/organizers', '♙'],
    ['Events', '/admin/events', '▣'],
    ['Approvals', '/admin/approvals', '✓'],
    ['Bookings', '/admin/bookings', '◆'],
    ['Payments', '/admin/payments', '▤'],
    ['Parking', '/admin/parking', 'Ⓟ'],
    ['Reports', '/admin/reports', '▥'],
    ['Categories', '/admin/categories', '◈'],
    ['Notifications', '/admin/notifications', '♢'],
    ['Settings', '/admin/settings', '⚙'],
  ];
  protected readonly organizerNav = [
    ['Dashboard', '/organizer/dashboard', '⌂'],
    ['My Events', '/organizer/my-events', '▣'],
    ['Create Event', '/organizer/events/create/type', '＋'],
    ['Approvals', '/organizer/approvals', '✓'],
    ['Bookings', '/organizer/events/summer/bookings', '◆'],
    ['Reports', '/organizer/reports', '▥'],
    ['Notifications', '/organizer/notifications', '♢'],
    ['Profile', '/organizer/profile', '♙'],
    ['Settings', '/organizer/settings', '⚙'],
  ];

  constructor(protected readonly router: Router) {
    this.sync(this.router.url);
    this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe((e) => {
        this.sync(e.urlAfterRedirects);
        this.sidebarOpen.set(false);
        window.scrollTo({ top: 0, behavior: 'smooth' });
      });
  }

  private sync(url: string): void {
    const clean = url.split('?')[0].replace(/^\//, '');
    this.role.set(clean.startsWith('admin') ? 'admin' : 'organizer');
    const parts = clean.split('/');
    if (this.role() === 'organizer') {
      if (clean === 'organizer' || clean === 'organizer/dashboard') this.view.set('dashboard');
      else if (clean === 'organizer/events/create') {
        this.activeWizardStep.set(1);
        this.view.set('create-type');
      } else if (clean === 'organizer/my-events') this.view.set('my-events');
      else if (clean.includes('/events/create/type')) {
        this.activeWizardStep.set(1);
        this.view.set('create-type');
      } else if (clean.includes('/events/create/details')) {
        this.activeWizardStep.set(2);
        this.view.set('create-details');
      } else if (clean.includes('/events/create/seats-or-capacity')) {
        this.activeWizardStep.set(3);
        this.view.set('create-seats');
      } else if (clean.includes('/events/create/ticket-pricing')) {
        this.activeWizardStep.set(4);
        this.view.set('create-pricing');
      } else if (clean.includes('/events/create/parking')) {
        this.activeWizardStep.set(5);
        this.view.set('create-parking');
      } else if (clean.includes('/events/create/poster-qr')) {
        this.activeWizardStep.set(6);
        this.view.set('create-poster');
      } else if (clean.includes('/events/create/review')) {
        this.activeWizardStep.set(7);
        this.view.set('create-review');
      } else if (clean.includes('/events/create/submit')) {
        this.activeWizardStep.set(8);
        this.view.set('create-submit');
      } else if (clean.endsWith('/seats')) this.view.set('seat-manage');
      else if (clean.endsWith('/ticket-pricing')) this.view.set('pricing-manage');
      else if (clean.endsWith('/parking-setup')) this.view.set('parking-manage');
      else if (clean.endsWith('/bookings')) this.view.set('event-bookings');
      else if (clean.endsWith('/reports')) this.view.set('event-reports');
      else if (clean.endsWith('/poster-qr')) this.view.set('poster-qr');
      else if (clean.endsWith('/edit')) this.view.set('event-edit');
      else if (clean === 'organizer/approvals') this.view.set('approvals');
      else if (clean === 'organizer/notifications') this.view.set('notifications');
      else if (clean === 'organizer/reports') this.view.set('reports');
      else if (clean === 'organizer/profile') this.view.set('profile');
      else if (clean === 'organizer/settings') this.view.set('settings');
      else if (parts[1] === 'events' && parts.length >= 3) this.view.set('event-detail');
      else this.view.set('dashboard');
    } else {
      if (clean === 'admin' || clean === 'admin/dashboard') this.view.set('dashboard');
      else if (clean === 'admin/properties') this.view.set('properties');
      else if (clean === 'admin/properties/create') this.view.set('property-form');
      else if (clean.endsWith('/edit') && clean.startsWith('admin/properties/'))
        this.view.set('property-form');
      else if (/^admin\/properties\/[^/]+$/.test(clean)) this.view.set('property-detail');
      else if (clean === 'admin/organizers') this.view.set('organizers');
      else if (/^admin\/organizers\/[^/]+$/.test(clean)) this.view.set('organizer-detail');
      else if (clean === 'admin/events') this.view.set('events');
      else if (clean === 'admin/events/create') this.view.set('admin-event-form');
      else if (/^admin\/events\/[^/]+\/layout$/.test(clean)) this.view.set('admin-layout');
      else if (/^admin\/events\/[^/]+$/.test(clean)) this.view.set('admin-event-detail');
      else if (clean === 'admin/approvals') this.view.set('admin-approvals');
      else if (/^admin\/approvals\/[^/]+$/.test(clean)) this.view.set('approval-detail');
      else if (clean === 'admin/bookings') this.view.set('admin-bookings');
      else if (/^admin\/bookings\/[^/]+$/.test(clean)) this.view.set('booking-detail');
      else if (clean === 'admin/payments') this.view.set('admin-payments');
      else if (/^admin\/payments\/[^/]+$/.test(clean)) this.view.set('payment-detail');
      else if (clean === 'admin/parking') this.view.set('admin-parking');
      else if (clean === 'admin/reports') this.view.set('admin-reports');
      else if (clean === 'admin/categories') this.view.set('categories');
      else if (clean === 'admin/notifications') this.view.set('admin-notifications');
      else if (clean === 'admin/settings') this.view.set('admin-settings');
      else this.view.set('dashboard');
    }
  }

  protected go(path: string): void {
    void this.router.navigateByUrl(path);
  }
  protected goToNotifications(): void {
    this.go(this.role() === 'admin' ? '/admin/notifications' : '/organizer/notifications');
  }
  protected goToAccount(): void {
    this.go(this.role() === 'admin' ? '/admin/settings' : '/organizer/profile');
  }
  protected flash(message: string): void {
    this.toast.set(message);
    window.setTimeout(() => this.toast.set(''), 2200);
  }
  protected openAction(name: string, mode: 'approve' | 'reject' | 'delete'): void {
    this.selectedName.set(name);
    this.modalMode.set(mode);
    this.showModal.set(true);
  }
  protected confirmAction(): void {
    this.flash(
      `${this.selectedName()} ${this.modalMode() === 'approve' ? 'approved' : this.modalMode() === 'reject' ? 'rejected' : 'removed'} successfully`,
    );
    this.showModal.set(false);
  }
  protected isActive(path: string): boolean {
    return (
      this.router.url.split('?')[0] === path ||
      (path.endsWith('/dashboard') &&
        (this.router.url === '/admin' || this.router.url === '/organizer'))
    );
  }
  protected seatLabel(index: number): string {
    return `${String.fromCharCode(65 + Math.floor(index / 8))}${(index % 8) + 1}`;
  }
  protected slotLabel(index: number): string {
    return `${String.fromCharCode(65 + Math.floor(index / 12))}${(index % 12) + 1}`;
  }
  protected selectSeat(index: number): void {
    this.selectedSeatIndex.set(this.selectedSeatIndex() === index ? null : index);
  }
  protected selectParking(index: number): void {
    this.selectedParkingIndex.set(this.selectedParkingIndex() === index ? null : index);
  }

  protected nextWizard(): void {
    const paths = [
      '/organizer/events/create/type',
      '/organizer/events/create/details',
      '/organizer/events/create/seats-or-capacity',
      '/organizer/events/create/ticket-pricing',
      '/organizer/events/create/parking',
      '/organizer/events/create/poster-qr',
      '/organizer/events/create/review',
      '/organizer/events/create/submit',
    ];
    const i = Math.min(this.activeWizardStep(), 7);
    this.go(paths[i]);
  }
  protected prevWizard(): void {
    const paths = [
      '/organizer/events/create/type',
      '/organizer/events/create/details',
      '/organizer/events/create/seats-or-capacity',
      '/organizer/events/create/ticket-pricing',
      '/organizer/events/create/parking',
      '/organizer/events/create/poster-qr',
      '/organizer/events/create/review',
      '/organizer/events/create/submit',
    ];
    const i = Math.max(this.activeWizardStep() - 2, 0);
    this.go(paths[i]);
  }
}
