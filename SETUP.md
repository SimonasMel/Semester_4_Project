# Project Setup Instructions

## Prerequisites
- .NET 10 SDK
- Visual Studio 2026 (or any .NET 10 IDE)

## Database Setup
The database is automatically created on first run. No manual setup is needed!

### Manual Database Creation (if needed)
1. Open the BackEnd project directory in PowerShell
2. Run: `dotnet ef database update`

## Running the Project
1. **Backend (API)**: `dotnet run` in the `BackEnd/` directory
   - Runs on: https://localhost:7065

2. **Frontend (Blazor)**: `dotnet run` in the `FrontEnd/` directory
   - Runs on: https://localhost:7140

The frontend will automatically connect to the backend API.
