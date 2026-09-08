import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-public-pages',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './public-pages.html',
  styleUrl: './public-pages.css',
})
export class PublicPages {
  protected readonly view: string;
  protected readonly query = signal('');
  protected readonly category = signal('All Categories');
  protected readonly sortAscending = signal(false);
  protected readonly favoriteIds = signal(new Set<string>());
  protected readonly selectedParking = signal('B27');
  protected readonly contactMessage = signal('');
  protected readonly publicEvents = [
    {
      id: 'summer',
      n: 'Summer Music Fest',
      cat: 'Concert',
      date: '24 MAY',
      fullDate: '24 May 2026',
      v: 'SoundWave Arena',
      p: '₹799',
      img: '/assets/concert-hero.webp',
      type: 'Seat Based',
    },
    {
      id: 'league',
      n: 'Football League Final',
      cat: 'Sports',
      date: '06 AUG',
      fullDate: '06 August 2026',
      v: 'Marina Arena',
      p: '₹1,299',
      img: '/assets/event-grid.webp',
      type: 'Seat Based',
    },
    {
      id: 'tech',
      n: 'Tech Conference 2026',
      cat: 'Conference',
      date: '18 JUL',
      fullDate: '18 July 2026',
      v: 'Trade Centre',
      p: '₹999',
      img: '/assets/eventora-event-sprite.png',
      type: 'Seat Based',
    },
    {
      id: 'food',
      n: 'Food Carnival 2026',
      cat: 'Festival',
      date: '10 JUN',
      fullDate: '10 June 2026',
      v: 'Island Grounds',
      p: '₹399',
      img: '/assets/event-grid.webp',
      type: 'General Entry',
    },
  ];
  protected currentEvent = this.publicEvents[0];
  protected readonly filteredEvents = computed(() => {
    const q = this.query().trim().toLowerCase();
    const category = this.category();
    const rows = this.publicEvents.filter(
      (event) =>
        (!q || `${event.n} ${event.v} ${event.cat}`.toLowerCase().includes(q)) &&
        (category === 'All Categories' || event.cat === category),
    );
    return this.sortAscending() ? [...rows].sort((a, b) => a.n.localeCompare(b.n)) : rows;
  });
  protected readonly faqs = [
    [
      'How do I book an event?',
      'Choose an event, select tickets and seats when applicable, optionally add parking, then complete the simulated payment.',
    ],
    [
      'Is parking mandatory?',
      'No. Event parking is optional and you can complete your ticket booking without a parking slot.',
    ],
    [
      'Can I cancel a booking?',
      'Yes. Open My Bookings, choose the booking and use Cancel Booking. Seats and parking are released after cancellation.',
    ],
    [
      'Where can I find my QR ticket?',
      'Your QR is available on the booking confirmation and ticket detail screens after payment is completed.',
    ],
  ];
  protected readonly openFaq = signal(0);

  constructor(protected readonly router: Router) {
    const clean = router.url.split('?')[0];
    this.currentEvent =
      this.publicEvents.find((event) => clean.endsWith('/' + event.id)) ?? this.publicEvents[0];
    if (clean === '/events') this.view = 'events';
    else if (clean.startsWith('/events/')) this.view = 'event-detail';
    else if (clean === '/how-it-works') this.view = 'how';
    else if (clean === '/parking') this.view = 'parking';
    else if (clean === '/contact') this.view = 'contact';
    else if (clean === '/faq') this.view = 'faq';
    else this.view = 'about';
  }
  protected go(path: string): void {
    void this.router.navigateByUrl(path);
  }
  protected toggleFavorite(id: string): void {
    const next = new Set(this.favoriteIds());
    next.has(id) ? next.delete(id) : next.add(id);
    this.favoriteIds.set(next);
  }
  protected searchEvents(): void {
    document.querySelector('.public-events')?.scrollIntoView({ behavior: 'smooth' });
  }
  protected toggleSort(): void {
    this.sortAscending.update((value) => !value);
  }
  protected chooseParking(slot: string): void {
    this.selectedParking.set(slot);
  }
  protected sendMessage(): void {
    this.contactMessage.set('Thanks — your message has been sent. We’ll reply shortly.');
  }
}
