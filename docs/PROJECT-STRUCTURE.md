# BRD-Aligned Project Structure

This document maps the planned repository layout to the Event & Parking Reservation System BRD v2.0 and the supplied Organizer/Admin/Customer system flow. It defines structure and ownership only; it does not implement business features.

## Repository layout

```text
EventParkingReservationSystem/
|-- solution/
|   `-- EventParkingReservationSystem.sln
|-- backend/
|   `-- EventParkingReservationSystem.API/
|       |-- Controllers/
|       |   |-- Core/
|       |   |-- Events/
|       |   |-- Transactions/
|       |   `-- Dashboards/
|       |-- Models/
|       |   |-- Core/
|       |   |-- Events/
|       |   `-- Transactions/
|       |-- DTOs/
|       |-- Interfaces/
|       |   |-- Services/
|       |   `-- Repositories/
|       |-- Services/
|       |-- Repositories/
|       |-- Validators/
|       |-- Mapping/
|       |-- Data/
|       |-- Middleware/
|       |-- Helpers/
|       |-- Exceptions/
|       |-- Extensions/
|       `-- Migrations/
|-- frontend/
|   `-- event-parking-reservation-ui/
|       `-- src/
|           |-- app/
|           |   |-- core/
|           |   |   |-- models/
|           |   |   |-- services/
|           |   |   |-- guards/
|           |   |   |-- interceptors/
|           |   |   `-- state/
|           |   |-- shared/
|           |   |   |-- components/
|           |   |   |-- directives/
|           |   |   `-- pipes/
|           |   `-- features/
|           |       |-- auth/
|           |       |-- customers/
|           |       |-- organizers/
|           |       |-- properties/
|           |       |-- venues/
|           |       |-- categories/
|           |       |-- events/
|           |       |-- event-approvals/
|           |       |-- seats/
|           |       |-- tickets/
|           |       |-- parking/
|           |       |-- bookings/
|           |       |-- payments/
|           |       |-- otp/
|           |       |-- qr-codes/
|           |       |-- notifications/
|           |       |-- reports/
|           |       |-- dashboards/
|           |       |-- admin/
|           |       |-- organizer/
|           |       |-- customer/
|           |       `-- not-found/
|           |-- assets/
|           `-- environments/
|-- database/
|   |-- scripts/
|   |   |-- schema/
|   |   |-- constraints/
|   |   `-- indexes/
|   |-- seed/
|   `-- ER-Diagram/
`-- docs/
    |-- API/
    |-- BRD/
    |-- Database/
    |-- Frontend/
    |-- System-Flow/
    `-- Testing/
```

## Backend module ownership

### Core

Authentication and authorization, customers, organizers, properties, venues, categories, JWT support, middleware and common exceptions.

### Events

Seat-based and non-seat-based events, event details, approval workflow, seat allocation, capacity/ticket allocation, ticket types and prices, parking allocation, posters and event QR setup.

### Transactions

Customer bookings, selected seats, optional parking reservations, payment simulation, OTP verification, booking QR codes, notifications and reports.

### Dashboards

Customer summaries and administrator statistics such as events, bookings, seats, parking occupancy, revenue and customers.

## Frontend boundaries

- `core/` holds singleton services, typed models, guards, interceptors and shared application state.
- `shared/` holds reusable UI components, directives and pipes.
- `features/` holds one area per BRD module plus Admin, Organizer and Customer route shells.
- The frontend must remain outside `backend/EventParkingReservationSystem.API/`.
- A future Angular workspace should use standalone components, Angular Router, HttpClient and Angular Forms.

## Team branches

- `member1-core-auth` owns Core/Auth/Admin base work.
- `member2-events` owns Events, approvals, tickets, seats and parking allocation.
- `member3-booking-payment` owns bookings, payments, OTP, QR, notifications and reports.
- `member4-frontend` owns the complete Angular frontend.

All member work should merge into `develop`. Nothing should be promoted to `main` until the integrated system is complete and approved.
