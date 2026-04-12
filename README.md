# MMS — Meeting Management System

Standalone Meeting Management System built for the **Ministry of Investment (MISA)**, Saudi Arabia.

## Overview

A comprehensive system for managing internal councils, committees, meetings, sessions, voting, recommendations, and procurement workflows.

- **Backend**: ASP.NET Core 10 (Web API) with EF Core 10 / SQL Server
- **Frontend**: Vue 3 + TypeScript + Vite + Tailwind CSS + PrimeVue
- **Database**: SQL Server with separate databases for app, audit, chat, viewer, and storage
- **Auth**: Local username/password with BCrypt + JWT, RBAC (User → Group / Role → Permission)
- **Realtime**: SignalR for in-meeting collaboration
- **Cache**: Redis for sessions and tokens

## Repository Structure

```
MMS/
├── MMS.sln                      # Visual Studio solution
├── MMSSolution/                 # Backend (.NET)
│   ├── MMS.API/                 # ASP.NET Core Web API
│   ├── MMS.BLL/                 # Business logic layer
│   ├── MMS.DAL/                 # Data access layer (EF Core)
│   ├── MMS.DTO/                 # Data transfer objects
│   └── Intalio.Tools.Common/    # Shared utilities
└── mms-portal/                  # Frontend (Vue 3)
    ├── src/                     # Source code
    ├── public/                  # Static assets
    └── ...
```

## Prerequisites

- .NET 10 SDK
- Node.js 20+
- SQL Server 2019+ (or SQL Server 2022)
- Redis server
- Visual Studio 2022 (optional, for IDE)

## Database Setup

Required databases on your SQL Server instance:
- `Intalio_MMS` — main application database
- `Intalio_MMS_Chat` — chat database
- `MMS_Viewer` — document viewer cache
- `MMS_Storage` — file storage (when using DB storage type)
- `MMS_Audit_Trail` — audit logs

Update the connection strings in the **`AppSettings`** table of the main DB and in `MMSSolution/MMS.API/appsettings.json`.

## Running

### Backend
```bash
cd MMSSolution
dotnet restore
dotnet build
dotnet run --project MMS.API
```
API will be available at `http://localhost:6565` (configurable).

### Frontend
```bash
cd mms-portal
npm install
npm run dev
```
UI will be available at `http://localhost:8084` (or as configured).

For production build:
```bash
npm run build
```

## Default Credentials

| Username | Password | Role |
|----------|----------|------|
| admin    | admin123 | SuperAdmin |
| user     | user123  | User |

## Features

- ✅ Local username/password authentication (BCrypt)
- ✅ Full RBAC: Users → Groups → Roles → Menu Permissions
- ✅ Internal & External Committees
- ✅ Meetings with attendees, agenda, voting (public/secret)
- ✅ Sessions with presentations
- ✅ Recommendations & tasks
- ✅ Multi-language (Arabic / English) with full RTL
- ✅ MISA-themed UI (`#006d4b` primary, `#63a58f` secondary)
- ✅ Calendar (Hijri & Gregorian)
- ✅ Real-time chat via SignalR
- ✅ Document viewer with PDF/Word/Excel/PowerPoint support
- ✅ Edit-in-Word with versioning
- ✅ Email & Outlook integration (SMTP / Microsoft Graph)
- ✅ Reports (attendance, committee summary)
- ✅ Audit trail
- ✅ Mobile responsive

## License

Proprietary — Ministry of Investment, Saudi Arabia.
