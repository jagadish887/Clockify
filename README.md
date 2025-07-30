
# TimeTracker - Professional Time Tracking System

A comprehensive Clockify-style time tracking application built with .NET Core MVC, Entity Framework Core, and ASP.NET Core Identity.

## Features

### 🔐 Authentication & Authorization
- ASP.NET Core Identity with role-based access control
- Three user roles: Admin, Manager, User
- Secure login/registration system

### ⏱️ Time Tracking
- Start/stop timer functionality
- Manual time entry creation and editing
- Project and task assignment
- Billable vs non-billable hour tracking
- Automatic duration calculation

### 📊 Project Management
- Client management (Admin/Manager only)
- Project organization with hourly rates
- Task management within projects
- Role-based access control

### 📈 Reporting & Analytics
- Comprehensive time reports with filtering
- Excel export functionality
- Dashboard with statistics (today, week, month)
- Project-wise breakdowns
- Role-based data access

### 🎨 Modern UI
- Bootstrap-based responsive design
- Bootstrap Icons integration
- Professional dashboard interface
- Mobile-friendly design

## Tech Stack

- **Backend**: ASP.NET Core MVC (.NET 8)
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Razor Pages with Bootstrap 5
- **Export**: ClosedXML for Excel generation
- **Icons**: Bootstrap Icons

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB, Express, or full version)
- Visual Studio 2022 or Visual Studio Code
- Git (optional)

## Getting Started

### 1. Clone or Download the Project

```bash
git clone <repository-url>
cd TimeTracker
```

### 2. Database Setup

The application uses SQL Server LocalDB by default. The connection string in `appsettings.json` is configured for LocalDB:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TimeTrackerDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

#### For LocalDB (Recommended for development):
Update the connection string to:
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TimeTrackerDb;Trusted_Connection=true;MultipleActiveResultSets=true"
```

#### For SQL Server Express:
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=TimeTrackerDb;Trusted_Connection=true;TrustServerCertificate=true;"
```

### 3. Install Entity Framework Tools (if not already installed)

```bash
dotnet tool install --global dotnet-ef
```

### 4. Run Database Migrations

```bash
cd TimeTracker
dotnet ef database update
```

This will create the database and apply the initial schema.

### 5. Build and Run the Application

#### Using Visual Studio:
1. Open `TimeTracker.sln` in Visual Studio
2. Set `TimeTracker` as the startup project
3. Press F5 or click "Start Debugging"

#### Using Command Line:
```bash
cd TimeTracker
dotnet run
```

The application will be available at:
- HTTPS: `https://localhost:7086`
- HTTP: `http://localhost:5086`

### 6. Default Login Credentials

The application automatically creates a default admin user:
- **Email**: admin@timetracker.com
- **Password**: Admin123!

## Project Structure

```
TimeTracker/
├── Controllers/           # MVC Controllers
│   ├── AccountController.cs
│   ├── ClientsController.cs
│   ├── HomeController.cs
│   ├── ProjectsController.cs
│   ├── ReportsController.cs
│   └── TimeEntryController.cs
├── Data/                  # Database Context
│   └── ApplicationDbContext.cs
├── Models/                # Entity Models
│   ├── AppUser.cs
│   ├── Client.cs
│   ├── Project.cs
│   ├── TaskItem.cs
│   ├── TimeEntry.cs
│   ├── Team.cs
│   ├── ProjectUser.cs
│   └── UserTeam.cs
├── ViewModels/            # View Models
│   ├── Account/
│   ├── Dashboard/
│   ├── Reports/
│   └── TimeEntry/
├── Views/                 # Razor Views
│   ├── Account/
│   ├── Home/
│   └── Shared/
├── wwwroot/              # Static files
├── Migrations/           # EF Core Migrations
├── Program.cs            # Application entry point
└── appsettings.json      # Configuration
```

## User Roles & Permissions

### Admin
- Full access to all features
- Manage all users, clients, projects
- View all time entries and reports
- Delete clients and projects

### Manager
- Manage clients and projects they created
- View team time entries (can be enhanced)
- Create and edit projects
- Generate reports for their team

### User (Default for new registrations)
- Track their own time entries
- View assigned projects
- Generate personal reports
- Cannot manage clients or projects

## Key Features Explained

### Time Tracking
- **Timer Mode**: Start/stop timers for real-time tracking
- **Manual Entry**: Add time entries with custom start/end times
- **Running Timer Detection**: Automatically stop previous timers when starting new ones

### Project Organization
- **Clients**: Top-level organization for businesses/organizations
- **Projects**: Belong to clients, have billable settings and hourly rates
- **Tasks**: Organize work within projects

### Reporting
- **Filtering**: By date range, user, project, client, billable status
- **Export**: Generate Excel files with detailed time data
- **Dashboard**: Real-time statistics and visual summaries

## Database Schema

The application uses Entity Framework Core with the following main entities:
- `AppUser` (extends IdentityUser)
- `Client`
- `Project`
- `TaskItem`
- `TimeEntry`
- `Team` / `UserTeam` / `ProjectUser` (for team management)

## Development Notes

### Adding New Features
1. Create/update models in `Models/` folder
2. Add migration: `dotnet ef migrations add <MigrationName>`
3. Update database: `dotnet ef database update`
4. Create ViewModels in `ViewModels/` folder
5. Implement controller actions
6. Create/update views

### Troubleshooting

#### Database Connection Issues
- Ensure SQL Server/LocalDB is running
- Check connection string in `appsettings.json`
- Verify database exists: `dotnet ef database update`

#### Migration Issues
```bash
# Remove last migration
dotnet ef migrations remove

# Create new migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License.

## Future Enhancements

- [ ] Team management features
- [ ] Invoicing system
- [ ] REST API for mobile apps
- [ ] Time off management
- [ ] Advanced reporting with charts
- [ ] Email notifications
- [ ] Audit logging
- [ ] Integration with external calendars
- [ ] Bulk time entry operations
- [ ] Time entry approval workflow
