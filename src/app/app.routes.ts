import { Routes } from '@angular/router';
import { Landing } from './pages/landing/landing';
import { Auth } from './pages/auth/auth';
import { NotFound } from './pages/not-found/not-found';
import { Portal } from './pages/portal/portal';
import { Management } from './pages/management/management';
import { PublicPages } from './pages/public-pages/public-pages';
import { adminGuard, customerGuard, organizerGuard } from './core/auth.guard';

const organizerPaths = [
  'organizer',
  'organizer/dashboard',
  'organizer/my-events',
  'organizer/events/create',
  'organizer/events/create/type',
  'organizer/events/create/details',
  'organizer/events/create/seats-or-capacity',
  'organizer/events/create/ticket-pricing',
  'organizer/events/create/parking',
  'organizer/events/create/poster-qr',
  'organizer/events/create/review',
  'organizer/events/create/submit',
  'organizer/events/:id',
  'organizer/events/:id/edit',
  'organizer/events/:id/seats',
  'organizer/events/:id/ticket-pricing',
  'organizer/events/:id/parking-setup',
  'organizer/events/:id/bookings',
  'organizer/events/:id/reports',
  'organizer/events/:id/poster-qr',
  'organizer/approvals',
  'organizer/notifications',
  'organizer/reports',
  'organizer/profile',
  'organizer/settings',
];

const adminPaths = [
  'admin',
  'admin/dashboard',
  'admin/properties',
  'admin/properties/create',
  'admin/properties/:id',
  'admin/properties/:id/edit',
  'admin/organizers',
  'admin/organizers/:id',
  'admin/events',
  'admin/events/create',
  'admin/events/:id/layout',
  'admin/events/:id',
  'admin/approvals',
  'admin/approvals/:id',
  'admin/bookings',
  'admin/bookings/:id',
  'admin/payments',
  'admin/payments/:id',
  'admin/parking',
  'admin/reports',
  'admin/categories',
  'admin/notifications',
  'admin/settings',
];

export const routes: Routes = [
  { path: '', component: Landing, title: 'Eventora | Events • Tickets • Parking' },
  { path: 'login', component: Auth, title: 'Login | Event Parking Reservation System' },
  { path: 'register', component: Auth, title: 'Create Account | Event Parking' },
  { path: 'forgot-password', component: Auth, title: 'Forgot Password | Event Parking' },
  { path: 'reset-password', component: Auth, title: 'Reset Password | Event Parking' },
  { path: 'events', component: PublicPages, title: 'Discover Events | Event Parking' },
  { path: 'events/:id', component: PublicPages, title: 'Event Details | Event Parking' },
  { path: 'about', component: PublicPages, title: 'About | Event Parking' },
  { path: 'contact', component: PublicPages, title: 'Contact | Event Parking' },
  { path: 'how-it-works', component: PublicPages, title: 'How It Works | Event Parking' },
  { path: 'parking', component: PublicPages, title: 'Parking | Event Parking' },
  { path: 'faq', component: PublicPages, title: 'FAQ | Event Parking' },

  // Existing customer flow kept intact.
  {
    path: 'app/dashboard',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Dashboard | Event Parking',
  },
  {
    path: 'app/events',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Discover Events | Event Parking',
  },
  {
    path: 'app/events/summer',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Summer Music Fest | Event Parking',
  },
  {
    path: 'app/events/summer/tickets',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Select Tickets | Event Parking',
  },
  {
    path: 'app/events/summer/seats',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Select Seats | Event Parking',
  },
  {
    path: 'app/events/summer/parking',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Select Parking | Event Parking',
  },
  {
    path: 'app/checkout',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Checkout | Event Parking',
  },
  {
    path: 'app/payment',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Payment | Event Parking',
  },
  {
    path: 'app/confirmed',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Booking Confirmed | Event Parking',
  },
  {
    path: 'app/bookings',
    component: Portal,
    canActivate: [customerGuard],
    title: 'My Bookings | Event Parking',
  },
  {
    path: 'app/bookings/summer',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Booking Details | Event Parking',
  },
  {
    path: 'app/ticket/summer',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Your Ticket | Event Parking',
  },
  {
    path: 'app/parking',
    component: Portal,
    canActivate: [customerGuard],
    title: 'My Parking | Event Parking',
  },
  {
    path: 'app/payments',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Payment History | Event Parking',
  },
  {
    path: 'app/payments/receipt',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Payment Receipt | Event Parking',
  },
  {
    path: 'app/notifications',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Notifications | Event Parking',
  },

  // Role-based customer aliases matching the project route specification.
  {
    path: 'customer/dashboard',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Customer Dashboard | Event Parking',
  },
  {
    path: 'customer/events',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Discover Events | Event Parking',
  },
  {
    path: 'customer/events/:id',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Event Details | Event Parking',
  },
  {
    path: 'customer/bookings',
    component: Portal,
    canActivate: [customerGuard],
    title: 'My Bookings | Event Parking',
  },
  {
    path: 'customer/bookings/:id',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Booking Details | Event Parking',
  },
  {
    path: 'customer/parking',
    component: Portal,
    canActivate: [customerGuard],
    title: 'My Parking | Event Parking',
  },
  {
    path: 'customer/payments',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Payments | Event Parking',
  },
  {
    path: 'customer/payment-history',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Payment History | Event Parking',
  },
  {
    path: 'customer/payments/receipt',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Payment Receipt | Event Parking',
  },
  {
    path: 'customer/notifications',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Notifications | Event Parking',
  },
  {
    path: 'customer/profile',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Profile | Event Parking',
  },
  {
    path: 'customer/settings',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Settings | Event Parking',
  },
  {
    path: 'customer/support',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Support | Event Parking',
  },
  {
    path: 'customer/favorites',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Favorites | Event Parking',
  },
  {
    path: 'customer/booking/tickets/:eventId',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Select Tickets | Event Parking',
  },
  {
    path: 'customer/booking/seats/:eventId',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Select Seats | Event Parking',
  },
  {
    path: 'customer/booking/parking/:eventId',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Select Parking | Event Parking',
  },
  {
    path: 'customer/booking/summary/:eventId',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Booking Summary | Event Parking',
  },
  {
    path: 'customer/booking/payment/:eventId',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Payment | Event Parking',
  },
  {
    path: 'customer/booking/success/:bookingId',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Booking Confirmed | Event Parking',
  },
  {
    path: 'customer/booking/ticket/:bookingId',
    component: Portal,
    canActivate: [customerGuard],
    title: 'Ticket & QR | Event Parking',
  },

  // Role-based management areas.
  ...organizerPaths.map((path) => ({
    path,
    component: Management,
    canActivate: [organizerGuard],
    title: 'Organizer | Event Parking',
  })),
  ...adminPaths.map((path) => ({
    path,
    component: Management,
    canActivate: [adminGuard],
    title: 'Admin | Event Parking',
  })),

  { path: '**', component: NotFound, title: 'Page Not Found | Event Parking' },
];
