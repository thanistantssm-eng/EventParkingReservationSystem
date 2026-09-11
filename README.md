# Event Parking Reservation System API

ASP.NET Core 8 Web API for the Eventora customer, organizer and administrator applications.

## Architecture

`Controllers → Services → Repositories → EF Core → SQL Server`

The API implements authentication, profiles, properties and venues, categories, organizer-owned events, approval/publication lifecycle, ticket types, seats, event parking, bookings, payments, OTP, QR codes, notifications, dashboards and reports.

## Run locally

From the repository root:

```bash
dotnet restore solution/EventParkingReservationSystem.sln
dotnet ef database update --project backend/EventParkingReservationSystem.API
dotnet run --project backend/EventParkingReservationSystem.API --urls http://localhost:5118
```

Health check: `http://localhost:5118/api/health`

The default development database uses SQL Server LocalDB. Override the connection string for another SQL Server environment.

## Production database configuration

Configure the hosted SQL Server connection string as an environment variable or hosting setting. Do not replace the LocalDB development value in `appsettings.json` and do not commit production credentials.

```text
ConnectionStrings__DefaultConnection=Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;
Database__ApplyMigrationsOnStartup=true
ASPNETCORE_ENVIRONMENT=Production
```

On startup, the API validates that `DefaultConnection` is present, retries transient SQL Server connection failures, and applies pending EF Core migrations. `/api/health` returns HTTP 200 only when both the API and database are reachable; otherwise it returns HTTP 503.

## Email OTP

Configure `Email:Host`, `Email:Port`, `Email:Username`, `Email:Password`, `Email:FromEmail`, and `Email:FromName` through .NET user secrets or environment variables. Do not commit SMTP credentials.

## Authorization model

- Public users can read only published event details and event-scoped availability.
- Customers can access only their own bookings, payments, receipts, QR records and profile.
- Organizers can manage and report on events owned by their organizer account.
- Administrators can review/publish events and access platform-wide operational records.

## Verify

```bash
dotnet build solution/EventParkingReservationSystem.sln
dotnet test solution/EventParkingReservationSystem.sln
```

The connected Angular frontend is maintained separately at `https://github.com/thanistantssm-eng/event-frontend`.
