
# Clockify Clone - Time Tracking System in .NET Core MVC

This project is a feature-rich clone of Clockify, built using ASP.NET Core MVC. It enables time tracking, project and task management, team collaboration, and reporting functionalities for individuals and teams.

## 🛠 Tech Stack

- **Frontend**: Razor Views with Bootstrap 5
- **Backend**: ASP.NET Core MVC (.NET 8)
- **ORM**: Entity Framework Core (Code-First)
- **Database**: SQL Server
- **Authentication**: ASP.NET Core Identity
- **Export**: ClosedXML (for Excel/CSV reports)

## 🧱 Features

### 🔑 Authentication & Roles
- User Registration/Login
- Role-based access (Admin, Manager, User)

### 🕒 Time Tracking
- Start/Stop timers or manual time entry
- Attach entries to projects/tasks
- Mark as billable or non-billable

### 📁 Projects & Clients
- Create/manage clients
- Create/manage projects linked to clients
- Billable flag for projects

### ✅ Tasks
- Projects consist of tasks
- Time entries linked to tasks

### 📊 Reports
- Summary and detailed time reports
- Group by project, user, client, or task
- Export to Excel

### 👥 Team Management (Optional)
- Assign users to teams
- Managers view team reports
- Admins manage all users

## 📦 Entity Models

- `AppUser` (inherits from `IdentityUser`)
- `Client`
- `Project`
- `TaskItem`
- `TimeEntry`

## 🧪 Testing

- Unit testing via xUnit or NUnit
- Focus on time calculations, reports, and role-based access

## 🚀 Future Features

- PDF Invoicing
- Time off management
- Google/Outlook calendar sync
- API support for mobile apps

## 📂 Project Structure

```
/Controllers
/Models
/Views
/Migrations
/Data
wwwroot/
Program.cs
Startup.cs
```

## 🏁 Getting Started

1. Clone the repository
2. Update `appsettings.json` with your DB connection string
3. Run migrations: `Update-Database`
4. Run the app: `dotnet run` or use Visual Studio

---

© 2025 YourCompany. Built with ❤️ for time tracking.
