# Group17 HabitTracker

A full-stack ASP.NET MVC web application for tracking personal habits with role-based access, calendar progress tracking, kanban workflow, admin user management, and demo data seeding.

## Overview

Habit Tracker is a portfolio project built with ASP.NET Core MVC, Entity Framework Core, and ASP.NET Identity.

The application allows users to:

- create and manage personal habits
- track progress in a monthly calendar
- organize habits visually in a kanban board
- view dashboard statistics
- use role-based access for user and admin scenarios

Admins can additionally:

- manage users
- activate or deactivate accounts
- seed demo data for presentation and testing

## Project Structure

```txt

HabitTrackerAspNetMVCWebApp/
├── Controllers/
├── Data/
├── Models/
├── Services/
├── ViewModels/
├── Views/
├── Areas/Identity/
├── wwwroot/
└── Migrations/

```


## Features

### Authentication and authorization

- ASP.NET Identity integration
- role-based access control
- default roles: `Admin`, `User`
- soft deactivation with `IsActive`
- inactive users cannot sign in

### Habit management

- create, edit, view, and delete habits
- support for daily and weekly frequency
- track habit status and kanban status
- user-specific data isolation

### Dashboard

- personalized statistics for the signed-in user
- total habits
- active habits
- completed habits
- recent habits overview

### Calendar

- monthly calendar view
- planned habits by date
- log habit completion status:
  - Completed
  - PartiallyCompleted
  - Skipped
- clear log action
- mark all habits for today as completed
- compact icon-based actions in a soft UI style

### Kanban board

- visual habit workflow
- grouped by kanban status
- easier progress tracking for active work

### Admin tools

- admin user management screen
- create and edit users
- activate / deactivate users
- review habits by user
- cleanup orphan habits
- seed realistic demo data from the UI

### Demo-ready setup

- automatic database migration on startup
- automatic default admin creation
- demo data seeding for presentation

## Tech Stack

- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQL Server / LocalDB
- ASP.NET Identity
- Razor Views
- Bootstrap
- Bootstrap Icons
- Custom CSS inspired by Materio-style admin UI

## Default Admin

On first startup, the application automatically creates the default admin account:

- Email: `adityapradhan5556@gmail.com`
- Password: `projectPass!12`

## Demo User

The demo data seeder also creates a demo regular user:

- Email: `demo.user@habittracker.local`
- Password: `Qw123456$`

## Getting Started

### Prerequisites

- .NET SDK
- SQL Server LocalDB or SQL Server
- Visual Studio 2022 or newer

### Run locally

1. Clone the repository
2. Open the solution
3. Update the connection string in `appsettings.json` if needed
4. Run the application

The app will:

- apply migrations automatically
- create required roles
- create the default admin account

### Reset the database

If you want to rebuild the database from scratch:

```txt
dotnet ef database drop --force
dotnet ef database update
```

