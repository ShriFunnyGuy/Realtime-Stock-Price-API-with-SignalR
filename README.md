# Stock Realtime

Realtime stock price API with SignalR push updates, PostgreSQL persistence, and a simple browser UI.

Projects
- Stock.RealTime.Api: ASP.NET Core (.NET 9) Web API + SignalR hub
- Stock.Models: Shared DTOs/models
- Stock.ServiceDefaults: Common hosting/instrumentation defaults (OpenTelemetry, etc.)
- Stock.AppHost: Optional Microsoft Aspire AppHost (for orchestration)
- Stock-UI: Static HTML/JS client using SignalR

Features
- REST: Get latest price by ticker (cached to DB)
- SignalR: Live price updates via hub groups per ticker
- PostgreSQL: Auto-create database/table on startup
- Swagger/OpenAPI (Development)
- Configurable update interval and volatility

Requirements
- .NET SDK 9
- PostgreSQL 14+ (local or container)
- Node/npm only if you want to serve the UI via a local web server (optional)

Configuration
Create appsettings.Development.json for Stock.RealTime.Api (or use environment variables):

CORS
Program.cs allows browser origins:
- http://localhost:8000
- http://127.0.0.1:8000

If your UI runs on a different origin, add it to WithOrigins(...). AllowCredentials is enabled; do not use AllowAnyOrigin with credentials.

Running the API
- Visual Studio 2022: Set startup project to Stock.RealTime.Api and run.
- CLI: dotnet run --project Stock.RealTime.Api

The API listens on the usual Kestrel ports; check console output. In Development:
- Swagger UI: http://localhost:<port>/swagger
- OpenAPI JSON: http://localhost:<port>/openapi/v1.json

SignalR
- Hub path: /stocks-feed
- Client method invoked by server: ReceivedStockPriceUpdate(update)
- Subscribe/unsubscribe methods: SubscribeToTicker(ticker), UnsubscribeFromTicker(ticker)

PostgreSQL
- The DatabaseInitializer will:
  - Ensure the database exists (by connecting to “postgres” then creating your DB if missing)
  - Create table public.stock_prices with indices if missing

Schema
- stock_prices(id serial PK, ticker varchar(10), price numeric(12,6), timestamp timestamp)

Frontend (Stock-UI)
1) Open Stock-UI/script.js and set:
   - config.apiRootUrl to your API base URL (e.g., http://localhost:5088)
2) Serve the folder on http://localhost:8000 (to match CORS), for example:
   - npx http-server ./Stock-UI -p 8000 --cors

Testing the hub with Postman (optional)
- Negotiate: POST http://localhost:<port>/stocks-feed/negotiate?negotiateVersion=1
- Open WebSocket: ws://localhost:<port>/stocks-feed?id=<connectionToken>
- Send handshake as first message: {"protocol":"json","version":1} + ASCII 0x1E terminator

Troubleshooting
- CORS with credentials: If the browser says Access-Control-Allow-Origin must not be *, ensure Program.cs uses WithOrigins(...) and AllowCredentials(), and your UI uses the exact origin listed.
- “No client method ‘ReceivedStockPriceUpdate’”: Register connection.on("ReceivedStockPriceUpdate", ...) before connection.start().
- DB/table not found: Ensure your connection string is correct. DatabaseInitializer runs at startup to create the DB/table if missing.
- Groups reset on reconnect: Re-subscribe to tickers after reconnect (SignalR drops group membership).

Project commands
- Restore: dotnet restore
- Build: dotnet build
- Run API: dotnet run --project Stock.RealTime.Api
- Run AppHost (if used): dotnet run --project Stock.AppHost

Security
- Do not commit API keys or production connection strings.
- Prefer environment variables or user-secrets in development.

License
- Add your license here.