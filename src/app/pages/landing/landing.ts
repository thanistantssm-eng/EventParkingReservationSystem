import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

type EventCard = {
  id: string;
  day: string;
  month: string;
  category: string;
  title: string;
  venue: string;
  time: string;
  price: string;
  ticketType: string;
  imagePosition: string;
};

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink],
  styleUrl: './landing.css',
  templateUrl: './landing.html',
})
export class Landing {
  protected readonly menuOpen = signal(false);
  protected readonly query = signal('');
  protected readonly selectedCategory = signal('All Categories');
  protected readonly favoriteTitles = signal(new Set<string>());
  protected readonly searchMessage = signal('');
  protected readonly selectedParking = signal('B27');

  protected readonly events: EventCard[] = [
    {
      id: 'summer',
      day: '24',
      month: 'MAY',
      category: 'Concert',
      title: 'AR Rahman Live in Chennai',
      venue: 'Jawaharlal Nehru Stadium, Chennai',
      time: '7:00 PM',
      price: '₹1,500',
      ticketType: 'Seat Based',
      imagePosition: '0% 34%',
    },
    {
      id: 'league',
      day: '10',
      month: 'JUN',
      category: 'Sports',
      title: 'IPL Finals 2025',
      venue: 'M.A. Chidambaram Stadium, Chennai',
      time: '7:30 PM',
      price: '₹2,000',
      ticketType: 'Seat Based',
      imagePosition: '33.333% 48%',
    },
    {
      id: 'tech',
      day: '15',
      month: 'JUL',
      category: 'Conference',
      title: 'Tech Summit 2026',
      venue: 'Chennai Trade Centre, Chennai',
      time: '10:00 AM',
      price: '₹999',
      ticketType: 'Non-Seat Based',
      imagePosition: '66.666% 45%',
    },
    {
      id: 'food',
      day: '28',
      month: 'AUG',
      category: 'Festival',
      title: 'Chennai Food Carnival',
      venue: 'Island Grounds, Chennai',
      time: '4:00 PM',
      price: '₹299',
      ticketType: 'Non-Seat Based',
      imagePosition: '100% 55%',
    },
  ];

  protected readonly filteredEvents = computed(() => {
    const query = this.query().trim().toLowerCase();
    const category = this.selectedCategory();
    return this.events.filter((event) => {
      const matchesQuery =
        !query || `${event.title} ${event.venue} ${event.category}`.toLowerCase().includes(query);
      const matchesCategory = category === 'All Categories' || event.category === category;
      return matchesQuery && matchesCategory;
    });
  });

  protected updateQuery(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
    this.searchMessage.set('');
  }

  protected updateCategory(event: Event): void {
    this.selectedCategory.set((event.target as HTMLSelectElement).value);
    this.searchMessage.set('');
  }

  protected findEvents(): void {
    const count = this.filteredEvents().length;
    this.searchMessage.set(
      count
        ? `${count} matching event${count === 1 ? '' : 's'} found.`
        : 'No matching events found.',
    );
    document.querySelector('#events')?.scrollIntoView({ behavior: 'smooth' });
  }

  protected focusSearch(): void {
    (document.querySelector('.search-field input') as HTMLInputElement | null)?.focus();
    document.querySelector('.search-wrap')?.scrollIntoView({ behavior: 'smooth' });
  }

  protected chooseCategory(category: string): void {
    this.selectedCategory.set(category);
    this.findEvents();
  }

  protected chooseParking(slot: string): void {
    this.selectedParking.set(slot);
  }

  protected toggleFavorite(title: string): void {
    const next = new Set(this.favoriteTitles());
    next.has(title) ? next.delete(title) : next.add(title);
    this.favoriteTitles.set(next);
  }
}
