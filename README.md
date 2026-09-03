# Event & Parking Reservation System — Backend Skeleton

This repository skeleton contains the ASP.NET Core Web API backend only.

**Frontend is intentionally NOT included.** The Admin, Organizer and Customer Angular dashboards will be created and maintained separately by the team.

## Structure

- `solution/EventParkingReservationSystem.sln` — Visual Studio solution
- `backend/EventParkingReservationSystem.API/` — ASP.NET Core Web API
- `database/` — SQL scripts, seed data and ER diagram work
- `docs/` — BRD, API documentation and system-flow references

## Backend layers

`Controllers -> Services -> Repositories -> Data -> SQL Server`

Supporting folders include DTOs, Interfaces, Middleware, Helpers, Validators, Mapping, Exceptions and Extensions.

## Planned system roles

- Admin
- Organizer
- Customer

## Planned backend modules

Authentication, users/customers, organizers, properties/venues, categories, events, event approvals, seats, ticket types, parking, bookings, payments, OTP verification, QR codes, notifications, reports and dashboards.

## Start

Open `solution/EventParkingReservationSystem.sln` in Visual Studio, restore NuGet packages, then run `EventParkingReservationSystem.API`.

Swagger opens in Development mode. Health endpoints:

- `/api/health`
- `/api/Health`

## Git workflow

Keep stable work in `main`, integration work in `develop`, and use feature branches for team work. Do not push unfinished feature work directly to `main`.
