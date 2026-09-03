# Backend Structure

The backend is a single ASP.NET Core Web API shared by the separately developed Angular applications.

```text
Angular Admin --------\
Angular Organizer ----- > ASP.NET Core Web API -> SQL Server
Angular Customer -----/
```

Backend folders:

- Controllers — HTTP endpoints
- Models — entities/domain models
- DTOs — request/response contracts
- Interfaces/Services — service contracts
- Interfaces/Repositories — repository contracts
- Services — business rules
- Repositories — database access
- Data — EF Core DbContext, entity configurations, seed data
- Middleware — global request/exception/auth middleware
- Helpers — JWT, QR, OTP, booking-number helpers
- Validators — request/business validation
- Mapping — entity/DTO mappings
- Exceptions — custom exceptions
- Extensions — dependency injection / Swagger / auth setup extensions
- Migrations — EF Core migrations

Planned module groups inside the layered folders:

- `Core` — authentication, customers, organizers, properties, venues and categories
- `Events` — seat-based/non-seat-based events, approvals, seats, ticket types and parking allocation
- `Transactions` — bookings, payments, OTP verification, QR codes, notifications and reports
- `Dashboards` — customer and administrator read-only summaries

These folders currently contain structure markers only. Feature classes will be added later on the owning member branches.
