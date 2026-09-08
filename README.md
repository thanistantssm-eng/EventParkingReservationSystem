# Eventora / Event Parking Reservation System UI

Premium responsive Angular frontend for the Event Parking Reservation System.

## Continuation status

This package continues the uploaded frontend rather than replacing its established visual language.

- Customer identity: orange
- Organizer identity: purple / violet
- Admin identity: green
- Shared premium SaaS cards, rounded panels, soft shadows, image-led sidebars/heroes, clean top bars, status chips and responsive layouts
- Existing landing/login/register/customer experience preserved
- Public, organizer and admin flows expanded so the project feels like one connected product family

## Completed public / authentication area

- Landing page
- Login with role-aware redirect
- Customer registration
- Forgot password / reset password demo flow
- 404 page
- Public Events listing
- Public Event Details
- About
- Contact
- How It Works
- Parking information
- FAQ

## Completed customer area

The original `/app/...` demo routes remain available for backward compatibility. Matching `/customer/...` aliases were added for the final route structure.

- Customer dashboard
- Discover Events
- Event Details
- Ticket type / quantity selection
- Seat selection
- Parking selection
- Booking summary / checkout
- Payment
- Booking confirmation
- Ticket / QR
- My Bookings
- Booking Details
- My Parking
- Payments / Payment History / Receipt
- Notifications
- Favorites
- Profile
- Settings
- Support

## Completed organizer area

- Organizer dashboard
- My Events
- 8-step Create Event wizard
  1. Event Type
  2. Event Details
  3. Seats / Capacity
  4. Ticket Type & Pricing
  5. Parking Allocation
  6. Poster / QR
  7. Review
  8. Submit for Approval
- Event detail / manage event
- Event edit
- Seat allocation editor
- Ticket / pricing setup
- Parking setup
- Poster / QR setup
- Approval status
- Event bookings
- Organizer reports / revenue
- Notifications
- Profile
- Settings

## Completed admin area

- Admin dashboard
- Property / venue list, create/edit and detail
- Organizer list and detail
- Event management
- Create event for organizer
- Event detail
- Seat / parking layout management
- Approval queue and approval review detail
- Approve / reject confirmation modal
- All bookings and booking detail
- All payments and payment detail
- Parking management with visual slot layout
- Event categories
- Reports & analytics
- Notifications
- Settings

## Reusable UX pieces

- Role guards using `sessionStorage` for the demo frontend
- Toast feedback in management flows
- Confirmation modal pattern
- Reusable status chips / badges
- Search and filter controls
- Responsive sidebar behavior
- Reusable `UiState` component for loading skeleton, empty and error states
- Responsive desktop / tablet / mobile CSS breakpoints

## Main route groups

### Public / auth

- `/`
- `/login`
- `/register`
- `/forgot-password`
- `/reset-password`
- `/events`
- `/events/:id`
- `/about`
- `/contact`
- `/how-it-works`
- `/parking`
- `/faq`

### Customer

- `/customer/dashboard`
- `/customer/events`
- `/customer/events/:id`
- `/customer/bookings`
- `/customer/bookings/:id`
- `/customer/parking`
- `/customer/payments`
- `/customer/payment-history`
- `/customer/notifications`
- `/customer/profile`
- `/customer/settings`
- `/customer/support`
- `/customer/favorites`
- `/customer/booking/tickets/:eventId`
- `/customer/booking/seats/:eventId`
- `/customer/booking/parking/:eventId`
- `/customer/booking/summary/:eventId`
- `/customer/booking/payment/:eventId`
- `/customer/booking/success/:bookingId`
- `/customer/booking/ticket/:bookingId`

### Organizer

- `/organizer/dashboard`
- `/organizer/my-events`
- `/organizer/events/create/type`
- `/organizer/events/create/details`
- `/organizer/events/create/seats-or-capacity`
- `/organizer/events/create/ticket-pricing`
- `/organizer/events/create/parking`
- `/organizer/events/create/poster-qr`
- `/organizer/events/create/review`
- `/organizer/events/create/submit`
- `/organizer/events/:id`
- `/organizer/events/:id/edit`
- `/organizer/events/:id/seats`
- `/organizer/events/:id/ticket-pricing`
- `/organizer/events/:id/parking-setup`
- `/organizer/events/:id/bookings`
- `/organizer/events/:id/reports`
- `/organizer/events/:id/poster-qr`
- `/organizer/approvals`
- `/organizer/notifications`
- `/organizer/reports`
- `/organizer/profile`
- `/organizer/settings`

### Admin

- `/admin/dashboard`
- `/admin/properties`
- `/admin/properties/create`
- `/admin/properties/:id`
- `/admin/properties/:id/edit`
- `/admin/organizers`
- `/admin/organizers/:id`
- `/admin/events`
- `/admin/events/create`
- `/admin/events/:id`
- `/admin/events/:id/layout`
- `/admin/approvals`
- `/admin/approvals/:id`
- `/admin/bookings`
- `/admin/bookings/:id`
- `/admin/payments`
- `/admin/payments/:id`
- `/admin/parking`
- `/admin/reports`
- `/admin/categories`
- `/admin/notifications`
- `/admin/settings`

## Demo login routing

Use the landing page role login links, or open directly:

- Customer: `/login` → `/customer/dashboard`
- Organizer: `/login?role=organizer` → `/organizer/dashboard`
- Admin: `/login?role=admin` → `/admin/dashboard`

The current frontend uses demo/session role state only. Replace this with the real JWT/ASP.NET Core auth service during backend integration.

## Run locally

```bash
npm install
npm start
```

Open `http://localhost:4200`.

## Framework / build note

The uploaded project already uses Angular `22.1.x`, TypeScript `6.0.x` and npm `11.17.0`. The continuation deliberately keeps that dependency line instead of silently downgrading the existing project to Angular 19.

A full Angular build could not be executed in the packaging container because its Node runtime is older than the engine requirement of the current Angular 22.1.7 toolchain. Source-level TypeScript syntax checks were run and no non-module TypeScript diagnostics were found. For local installation/build, use a Node version supported by the package versions in `package-lock.json` (for example a current Node 24 release), then run `npm install` and `npm run build`.

## Backend integration note

This package is still a frontend/demo-data implementation. Buttons, filters, wizard navigation, approval modals, status interactions and role navigation are wired client-side and are ready to be connected to the ASP.NET Core API through Angular services.

### UI readability update
The latest package includes a readability/alignment pass: larger labels and controls, clearer tables and sidebar text, improved spacing, and larger interaction targets while retaining the existing premium theme.
