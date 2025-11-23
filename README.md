# Red-Yellow-Green

Simple fullstack demo application that digitalizes equipment production states (red/yellow/green).
Workers can update states from a mobile device, dashboards update in real time, and supervisors can browse historical state changes.

**Features**
- **Local updates from mobile:** lightweight frontend allows workers to change equipment state.
- **Realtime overview dashboard:** Using SignalR hub broadcasts state changes to connected dashboard. Viewable for both supervisor and worker. 
- **API, history & reporting:** backend persists state changes (SQLite) for supervisor reports, and viewable for supervisors in extra dashbord.

**Architecture (high level)**
- **Frontend:** Vite + React app located in `frontend/` — mobile-friendly UI to view and change states.
- **Backend:** ASP.NET Core Web API in `backend/` — REST endpoints + SignalR hub for realtime updates.
- **Persistence:** SQLite database (used by Entity Framework Core migrations in `backend/`).
- **Realtime transport:** SignalR hub at `/hubs/state` for live updates between devices and dashboards.

Quick facts
- API endpoints: `GET /api/equipment`, `PATCH /api/equipment/state` (body: `{ equipmentId, newState, changedBy }`), `GET /api/equipment/{id}/history`.
- SignalR hub: `/hubs/state`.

Tech summary
| Layer | Languages / Frameworks |
|---|---|
| Frontend | JavaScript, React, Vite, `@microsoft/signalr` |
| Backend / API | C#, ASP.NET Core (net8.0), SignalR, EF Core (SQLite) |
| Database | SQLite (via EF Core) |
| Container / tooling | Docker, Docker Compose, npm, dotnet CLI |

Getting started (local, simple)
- Start backend (local SQLite DB):

```bash
dotnet run --project backend/RedYellowGreen.Api.csproj
```

- Start frontend (from `frontend/`) visit `http://localhost:3000`:

```bash
cd frontend
npm install
npm run dev
```

Run with Docker

```bash
docker compose up --build
```

What is demonstrated in one singel view for simplicity purpose:
- A simple mobile-friendly controller UI to change an equipment's state.
- A dashboard view that both workers and supervisors have access to, to view overall updates instantly when states change.
- For supervisors only, a historical view per equipment showing timestamped state changes and orders.

Why this design
- Uses standard web technologies so any modern phone or tablet can act as the local controller.
- SignalR provides low-latency updates without polling, keeping the dashboard in sync.
- SQLite keeps the setup simple for demos and interviews while EF Core migrations provide
	an upgrade path to a more robust DB if needed.

Next steps (ideas)
- Add authentication (workers vs supervisors).
- Add order scheduling and correlate orders with state changes.
- Add device pairing (QR codes) for physical equipment-to-device mapping.