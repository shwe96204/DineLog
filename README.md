# DineLog

DineLog is a restaurant journaling application built with ASP.NET Core, Blazor, MudBlazor, and SQLite. It lets users manage restaurants, restaurant diary entries, tags, OTP-based email verification, and a personal dashboard.

## Solution Structure

- `MyApplication`  
  Main web application host. This is the frontend entry point you open in the browser.
- `MyApplication.Client`  
  Blazor WebAssembly client UI components and pages.
- `MyApplication.Api`  
  Backend API for authentication, restaurants, diaries, tags, quotes, and email/OTP features.
- `Shared`  
  Shared models and services used across the solution.

## Minimum System Requirements

These are practical minimum specifications for development and local testing.

### Hardware

- CPU: Dual-core 64-bit processor, 2.0 GHz or better
- RAM: 8 GB minimum
- Storage: At least 2 GB free space for SDKs, NuGet packages, build output, and SQLite database files

### Operating System

- Windows 10 or Windows 11
- macOS 13 or later

### Software Prerequisites

- `.NET SDK 10.0` or later recommended
- `.NET 9 targeting support/runtime` available locally because the current solution mixes:
  - `MyApplication` -> `net9.0`
  - `MyApplication.Client` -> `net9.0`
  - `MyApplication.Api` -> `net10.0`
- A modern browser:
  - Google Chrome
  - Microsoft Edge
  - Firefox

### Recommended Development Tools

- Visual Studio 2022 or later
- Visual Studio Code with C# Dev Kit

## Before You Run

### 1. Install required .NET workloads

If you are using the CLI:

```bash
dotnet workload install wasm-tools
```

### 2. Download required .NET SDK

1. .NET9 SDK => https://dotnet.microsoft.com/en-us/download/dotnet/9.0
2. .NET10 SDK => https://dotnet.microsoft.com/en-us/download/dotnet/10.0

### 3. Download C# Dev Kit Extension

If you are using with Visual Studio Code, download C# Dev Kit in extension.

## How To Run The Application

This solution runs as two applications during development:

1. `MyApplication.Api` => `Program.cs`
2. `MyApplication`     => `Program.cs`

The frontend host is already configured to call the API at `http://localhost:5203`.

### Option A: Run with Visual Studio

1. Open [MyApplication.sln]
2. Set multiple startup projects:
   - `MyApplication.Api`
   - `MyApplication`
3. Start both projects.
4. Open the frontend in the browser:
   - `http://localhost:5025`

### Option B: Run with the .NET CLI

Open two terminals in the project root.

#### Terminal 1: Start the API

```bash
dotnet run --project MyApplication.Api
```

Expected local API URL:

- `http://localhost:5203`

#### Terminal 2: Start the web application

```bash
dotnet run --project MyApplication
```

Expected local app URL:

- `http://localhost:5025`

## First-Time Startup Notes

When the API starts for the first time:

- SQLite database initialization runs automatically
- the local database file is created from the configured connection string

Default API database connection string:

```json
"ConnectionStrings": {
  "DineLog": "Data Source=dine-log.db"
}
```

## Build Commands

To build the full solution:

```bash
dotnet build MyApplication.sln
```

To build projects individually:

```bash
dotnet build MyApplication.Api/MyApplication.Api.csproj
dotnet build MyApplication/MyApplication.csproj
dotnet build MyApplication.Client/MyApplication.Client.csproj
```

## Common Development URLs

- Frontend app: `http://localhost:5025`
- Frontend HTTPS profile: `https://localhost:7222`
- API: `http://localhost:5203`

## Troubleshooting

### The main page loads blank or shows a startup error

Check that:

- `MyApplication.Api` is running on `http://localhost:5203`
- `MyApplication` is running on `http://localhost:5025`
- both projects restored successfully

## Recommended Production-Oriented Specs

If you plan to demo the app, run builds frequently, or work with larger data and images, these specs will feel much better:

- CPU: Quad-core 64-bit processor
- RAM: 16 GB
- Storage: SSD with at least 5 GB free
- Browser: latest Chrome or Edge

