# Event & Parking Reservation System - Project Skeleton

This repository contains the BRD-aligned project skeleton for an ASP.NET Core Web API backend and a separately located Angular frontend.

**No frontend application or business feature has been implemented yet.** The root-level `frontend/` directory contains only an empty Angular-ready folder structure for future Admin, Organizer and Customer work.

## Structure

- `solution/EventParkingReservationSystem.sln` — Visual Studio solution
- `backend/EventParkingReservationSystem.API/` — ASP.NET Core Web API
- `frontend/event-parking-reservation-ui/` — empty Angular-ready structure (no Angular workspace yet)
- `database/` — SQL scripts, seed data and ER diagram work
- `docs/` — BRD, API documentation and system-flow references

See `docs/PROJECT-STRUCTURE.md` for the complete BRD-aligned folder map and module ownership.

## Backend layers

`Controllers -> Services -> Repositories -> Data -> SQL Server`

Supporting folders include DTOs, Interfaces, Middleware, Helpers, Validators, Mapping, Exceptions and Extensions.

## Planned system roles

- Admin
- Organizer
- Customer

## Planned backend modules

Authentication, customers, organizers, properties/venues, categories, events, event approvals, seats, ticket types, parking, bookings, payments, OTP verification, QR codes, notifications, reports and dashboards.

## Start

Open `solution/EventParkingReservationSystem.sln` in Visual Studio, restore NuGet packages, then run `EventParkingReservationSystem.API`.

Swagger opens in Development mode. Health endpoints:

- `/api/health`
- `/api/Health`

## Git workflow

Keep stable work in `main`, integration work in `develop`, and use feature branches for team work. Do not push unfinished feature work directly to `main`.
