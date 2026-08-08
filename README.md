# EVChargePlanner

⚡ A full-stack application that calculates the cheapest time windows to charge one or more electric vehicles, based on real day-ahead electricity prices from the Norwegian market — and reserves them so future plans respect what's already scheduled. Built as a portfolio project to practice algorithmic problem-solving, external API integration, and cloud deployment on top of a C#/.NET backend and a React/TypeScript frontend.

**🔗 Live demo:** [zealous-flower-034106c0f.7.azurestaticapps.net](https://zealous-flower-034106c0f.7.azurestaticapps.net)
**🔗 API:** [evchargeplanner-api.lemonmoss-4a4fa75f.northeurope.azurecontainerapps.io](https://evchargeplanner-api.lemonmoss-4a4fa75f.northeurope.azurecontainerapps.io)

> **Status: complete.** Deployed and running on Azure — frontend on Azure Static Web Apps (with continuous deployment via GitHub Actions), backend on Azure Container Apps, database on Azure Database for PostgreSQL.
>
> Note: to keep cloud costs near zero between demos, the database is normally kept **paused** and is only started when the app is being actively shown or tested. If the live demo above doesn't load, that's why — reach out and I'll spin it back up.

## What it does

- **Live electricity prices**: fetches real day-ahead prices for Norway (via [hvakosterstrommen.no](https://www.hvakosterstrommen.no/strompris-api), a public API built on Nord Pool data). A background service refreshes them automatically and starts pulling tomorrow's prices as soon as they're published, without any manual step.
- **Car catalog**: a curated catalog of 30 common EV/PHEV models (grouped by brand) autofills battery capacity and charging power when adding a car — the owner still picks their own name for it (e.g. "David's Car").
- **Multi-vehicle, multi-charger planning**: given one or more vehicles, their current/target battery percentage, an optional arrival time, and an optional departure deadline, it calculates the **cheapest charging window** for each — sharing a limited number of named chargers, prioritizing whichever vehicle has the tightest deadline, with minute-level precision (no unrealistic hour-boundary conflicts between vehicles).
- **Honest partial-charge feedback**: if a full charge isn't possible, the app says exactly how far the battery will get — and distinguishes *why*: a tight deadline vs. price data simply not being available that far ahead yet.
- **Persisted reservations**: confirming a calculated plan reserves those exact time slots per charger. The next plan calculated — for the same or a different vehicle — has to work around what's already reserved, just like a real shared charger would. Reservations can be reviewed and cancelled from the dashboard.
- **Live dashboard**: an hourly price chart (day/date labels appear automatically once tomorrow's prices are in) plus a table of today's reserved sessions with estimated cost, refreshed automatically every few minutes.

## Why this project

This is a deliberately more algorithm-heavy project than a typical CRUD app: the core of it is a scheduling problem (multiple vehicles competing for limited charging capacity, under time constraints, optimizing for cost, with real-world minute precision) rather than just reading and writing database records. It was also chosen for its relevance to the Norwegian market, where EV adoption is exceptionally high — and where electricity prices can genuinely swing from a few öre to several kroner per kWh within the same day.

## Tech stack

**Backend**
- C# / .NET 10, ASP.NET Core Web API
- Entity Framework Core + PostgreSQL
- JWT authentication (BCrypt for password hashing)
- `BackgroundService` for scheduled price fetching
- xUnit for algorithm and integration tests (9 tests covering the scheduling algorithm, multi-charger assignment, partial-charge reasoning, and reservation persistence)

**Frontend**
- React + TypeScript, Vite
- React Router, Axios
- Recharts (price chart)

**Tooling & Cloud**
- Docker & Docker Compose (local dev: API + PostgreSQL, automatic migrations on startup)
- GitHub Actions (CI for the backend; CD for the frontend via Static Web Apps)
- Azure Container Apps, Azure Container Registry, Azure Database for PostgreSQL, Azure Static Web Apps

## Architecture

The backend follows a layered structure, same approach as in an earlier project in this portfolio (FleetManager):

- `EVChargePlanner.Domain` — entities, the `IPriceProvider` abstraction, and the charging planner algorithm. No external dependencies.
- `EVChargePlanner.Infrastructure` — EF Core, the Norwegian price provider implementation, the background price-fetching service.
- `EVChargePlanner.Api` — controllers, authentication, HTTP layer.

`IPriceProvider` is designed so a second country's price source (Spain is planned) can be added as a new implementation without touching the rest of the app.

## The algorithm, briefly

- Finding the cheapest window in a day uses **minute-level precision** (not just hour blocks) — two vehicles can be scheduled back-to-back at, say, 17:23, without either being forced into an unrealistic full-hour slot.
- Planning for *multiple* vehicles sharing a limited, named set of chargers uses a **greedy strategy**: vehicles are ordered by deadline urgency (soonest first), and each claims the cheapest available slot — on a specific charger — that doesn't collide with an already-assigned vehicle or an existing confirmed reservation.
- If the ideal duration doesn't fit, the algorithm falls back to the **longest available slot** before the deadline, reports the resulting battery percentage, and flags *why* it fell short (tight deadline vs. price data not extending far enough).
- Confirmed plans are persisted as `ChargingSession` records, which become part of the "already reserved" state for every subsequent calculation.
- All of the above is covered by unit tests that assert on specific, non-obvious outcomes (not just "it returns something").

## Deployment

Deployed entirely on Azure:

| Piece | Service |
|---|---|
| Frontend | Azure Static Web Apps (continuous deployment on every push to `main`) |
| Backend | Azure Container Apps, image built and pushed to Azure Container Registry |
| Database | Azure Database for PostgreSQL (Flexible Server) |

The backend image is built for `linux/amd64` explicitly (`docker build --platform linux/amd64 ...`), since Azure's container hosts don't run ARM images — a detail that matters when building from an Apple Silicon Mac. The JWT signing key is kept out of source control (user-secrets locally, an environment variable in the container).

## Getting started (local development)

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Running with Docker (recommended)

```bash
docker compose up --build
```

API available at `http://localhost:8080`. Migrations, including the seeded car model catalog, are applied automatically on startup.

### Running locally

```bash
dotnet ef database update --project EVChargePlanner.Infrastructure --startup-project EVChargePlanner.Api
dotnet run --project EVChargePlanner.Api
```

### Running the frontend

```bash
cd evchargeplanner-client
npm install
npm run dev
```

## API overview

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Log in and receive a JWT |
| GET | `/api/cars` | List saved vehicles |
| POST / PUT / DELETE | `/api/cars/{id}` | Manage vehicles |
| GET | `/api/car-models` | Browse the EV/PHEV model catalog |
| GET / POST / DELETE | `/api/chargers` | Manage named chargers |
| GET | `/api/prices/upcoming` | Prices from 3 hours ago through the latest available data |
| GET | `/api/prices/availability` | How far ahead price data currently extends |
| POST | `/api/charging-plan` | Calculate the optimal charging plan for one or more vehicles |
| POST | `/api/charging-plan/confirm` | Persist a calculated plan as reserved charging sessions |
| GET | `/api/charging-plan/today` | List today's reserved sessions |
| DELETE | `/api/charging-plan/sessions/{id}` | Cancel a reserved session |

All endpoints except `/api/auth/*` require a valid JWT.

## Possible future improvements

- Second `IPriceProvider` implementation for Spain (REE/ESIOS)
- Move the JWT signing key into Azure Key Vault instead of a plain environment variable

## Notes

Built as the second project in a portfolio aimed at DAM (Desarrollo de Aplicaciones Multiplataforma) internship applications, following an earlier fleet-management project (FleetManager). Focused on going beyond basic CRUD into real algorithmic logic, external data integration, and a full cloud deployment.