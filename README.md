# EVChargePlanner

⚡ A full-stack application that calculates the cheapest time windows to charge one or more electric vehicles, based on real day-ahead electricity prices from the Norwegian market. Built as a learning project to practice algorithmic problem-solving, external API integration, and DevOps fundamentals on top of a solid C#/.NET backend and a React/TypeScript frontend.

> **Status: actively in development.** Core functionality (pricing, planning algorithm, auth, CRUD, CI) is working end-to-end. Some features described in "Roadmap" below are still being built.

## What it does

- Fetches real day-ahead electricity prices for Norway (via [hvakosterstrommen.no](https://www.hvakosterstrommen.no/strompris-api), a public API built on Nord Pool data), refreshed automatically by a background service.
- Lets users register their electric vehicles (battery capacity, max charging power).
- Given one or more vehicles, their current/target battery percentage, and an optional departure deadline, calculates the **cheapest continuous charging window** for each — sharing a limited number of chargers fairly, prioritizing whichever vehicle has the tightest deadline.
- If there isn't enough time before the deadline for a full charge, it tells the user how far the battery will realistically get instead of just failing silently.
- Visualizes today's hourly prices on a chart.

## Why this project

This is a deliberately more algorithm-heavy project than a typical CRUD app: the core of it is a scheduling problem (multiple vehicles competing for limited charging capacity, under time constraints, optimizing for cost) rather than just reading and writing database records. It was also chosen for its relevance to the Norwegian market, where EV adoption is exceptionally high.

## Tech stack

**Backend**
- C# / .NET 10, ASP.NET Core Web API
- Entity Framework Core + PostgreSQL
- JWT authentication (BCrypt for password hashing)
- `BackgroundService` for scheduled price fetching
- xUnit for algorithm and integration tests

**Frontend**
- React + TypeScript, Vite
- React Router, Axios
- Recharts (price chart)

**Tooling**
- Docker & Docker Compose (API + PostgreSQL, automatic migrations on startup)
- GitHub Actions (CI: build + test on every push)

## Architecture

The backend follows a layered structure, same approach as in [FleetManager](#), an earlier project in this portfolio:

- `EVChargePlanner.Domain` — entities, the `IPriceProvider` abstraction, and the charging planner algorithm. No external dependencies.
- `EVChargePlanner.Infrastructure` — EF Core, the Norwegian price provider implementation, the background price-fetching service.
- `EVChargePlanner.Api` — controllers, authentication, HTTP layer.

`IPriceProvider` is designed so a second country's price source (Spain is planned) can be added as a new implementation without touching the rest of the app.

## The algorithm, briefly

- Finding the cheapest N-hour window in a day is solved with a **sliding window** approach (O(n)).
- Planning for *multiple* vehicles sharing a limited number of chargers uses a **greedy strategy**: vehicles are ordered by deadline urgency (soonest first), and each claims the cheapest available slot that doesn't collide with an already-assigned vehicle.
- Both are covered by unit tests that assert on specific, non-obvious outcomes (not just "it returns something").

## Getting started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Running with Docker (recommended)

```bash
docker compose up --build
```

API available at `http://localhost:8080`. Migrations are applied automatically on startup.

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
| GET | `/api/prices/today` | Today's hourly electricity prices |
| POST | `/api/charging-plan` | Calculate the optimal charging plan for one or more vehicles |

All endpoints except `/api/auth/*` require a valid JWT.

## Roadmap / in progress

- [ ] Catalog of common EV/PHEV models to autofill battery specs when adding a car
- [ ] Live-updating price chart as new hourly prices become available
- [ ] Second `IPriceProvider` implementation for Spain (REE/ESIOS)
- [ ] Deployment to Azure with GitHub Actions CD
- [ ] Additional test coverage

## Notes

Built as the second project in a portfolio aimed at DAM (Desarrollo de Aplicaciones Multiplataforma) internship applications, following an earlier fleet-management project (FleetManager). Focused on going beyond basic CRUD into real algorithmic logic, external data integration, and containerized deployment.
