# MediReminder

MediReminder is a  full-stack application for organizing medicines, schedules, prescriptions, and dose history. It includes a React frontend, an ASP.NET Core Web API, and SQL Server persistence.

> demo project. Use synthetic data only. This application is not medical advice and is not intended to replace a clinician or a production healthcare system.

## Features

- Register and sign in with email or mobile number; Google sign-in is available after OAuth configuration.
- Add and manage medicines and dose schedules.
- Review dashboard, calendar, dose history, and missed doses.
- Upload and review prescription files.
- Manage profile, account settings, and light/dark/system appearance.

## Technology

- Frontend: React, Vite, React Router, Axios
- Backend: ASP.NET Core Web API on .NET 8, Entity Framework Core
- Database: Microsoft SQL Server
- API documentation: Swagger (development)

## Repository layout

```text
MediRemainder-frondend/          React + Vite client
MediReminder-Backend/
  MediReminderBackend/           ASP.NET Core API and EF Core migrations
global.json                      .NET SDK version pin
```

## Run locally

### Requirements

- .NET 8 SDK
- Node.js and npm
- SQL Server (local SQL Server or LocalDB)
- Optional: Google OAuth web client ID for Google sign-in

### 1. Configure SQL Server

Edit `MediReminder-Backend/MediReminderBackend/appsettings.json` and set `ConnectionStrings:DefaultConnection` to your SQL Server instance. The checked-in value uses Windows Authentication and the local default instance:

```text
Server=localhost;Database=MediReminderDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

For a local SQL Express instance, use `Server=.\\SQLEXPRESS` (JSON requires the doubled backslash). Keep passwords and production connection strings out of the repository; use .NET user secrets locally or environment variables in deployment.

### 2. Create or update the database

From PowerShell:

```powershell
cd MediReminder-Backend/MediReminderBackend
dotnet ef database update
```

### 3. Start the API

From the backend project directory:

```powershell
dotnet run --launch-profile http
```

The API listens on `http://localhost:5202`. Swagger is available at `http://localhost:5202/swagger`.

### 4. Start the frontend

In a second PowerShell window:

```powershell
cd MediRemainder-frondend
npm ci
Copy-Item .env.example .env.local
npm run dev
```

Open the local URL Vite prints (normally `http://localhost:5173`). The Vite development proxy forwards `/api` requests to `http://localhost:5202`.

## Google sign-in (optional)

Create a Web application OAuth client in Google Cloud and authorize the frontend origin(s) you use locally, normally `http://localhost:5173` and `http://localhost:5174`.

Set the same client ID in `MediRemainder-frondend/.env.local`:

```dotenv
VITE_GOOGLE_CLIENT_ID=your-web-client-id.apps.googleusercontent.com
```

Set the backend audience value using .NET user secrets from the backend project directory:

```powershell
dotnet user-secrets init
dotnet user-secrets set "Google:ClientId" "your-web-client-id.apps.googleusercontent.com"
```

Restart both applications after changing configuration. Do not commit `.env.local`, OAuth client secrets, or other credentials.

## API overview

The API is rooted at `/api`; endpoints include authentication, medicines, schedules, calendar, history, prescriptions, profile, and settings. Use Swagger locally to explore request and response schemas.

## Portfolio deployment notes

GitHub stores this repository's source code; it does not run the .NET API or SQL Server. A live deployment needs a hosted API and database. Configure the deployed frontend API URL, backend CORS origins, SQL connection string, JWT secret, and Google authorized origin through the hosting provider's environment settings. Do not use real personal or health data in a portfolio demo.
