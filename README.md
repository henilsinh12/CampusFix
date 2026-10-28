# CampusFix — Smart Campus Complaint & Maintenance Management System

A mini-project for **IT5039: Web Technology with .NET**, B.Tech Semester V, Uka Tarsadia University.

CampusFix is an ASP.NET Core MVC application for reporting campus facility problems, assigning maintenance work, and tracking complaint status.

## Features

- ASP.NET Core MVC with Razor Views
- MySQL persistence using Entity Framework Core and Pomelo
- ASP.NET Core Identity authentication and role-based authorization
- Student, Staff and Admin roles
- Complaint create/read/update/delete workflows
- Server-side model validation and anti-forgery protection
- Complaint reference numbers and status history
- Dashboard metrics, search and filters
- Category administration and staff assignment
- Session state and TempData notifications
- REST API endpoints for complaint retrieval, creation and status PATCH
- Responsive Bootstrap-based interface

## Technology stack

- .NET 8 / ASP.NET Core MVC
- C#
- Entity Framework Core 8
- MySQL 8+
- Pomelo.EntityFrameworkCore.MySql
- ASP.NET Core Identity
- Razor, HTML, CSS, JavaScript, Bootstrap 5

## Prerequisites

1. .NET 8 SDK
2. MySQL Server 8+ (or a compatible MySQL service)
3. VS Code or Visual Studio
4. Git (for GitHub submission)

## Setup in VS Code / PowerShell

### 1. Create a MySQL database and user

Log into MySQL as an administrator and run:

```sql
CREATE DATABASE campusfix_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'campusfix_user'@'localhost' IDENTIFIED BY 'Choose_A_Strong_Password';
GRANT ALL PRIVILEGES ON campusfix_db.* TO 'campusfix_user'@'localhost';
FLUSH PRIVILEGES;
```

### 2. Configure the connection string

Edit `appsettings.json` and replace `CHANGE_ME` in `ConnectionStrings:DefaultConnection` with the password you created. Do not commit real production passwords to GitHub.

### 3. Restore and run

Open a terminal in the folder containing `CampusFix.csproj`:

```powershell
dotnet restore
dotnet run
```

Open the HTTPS or HTTP URL shown in the terminal. The first run creates the database schema and seeds complaint categories, roles, and a development administrator account.

> This academic starter uses `EnsureCreated` to create the schema for a fresh database. If you later change entity schemas, use EF Core migrations and a fresh development database or add proper migrations; `EnsureCreated` does not update an existing schema.

## Development administrator account

- Email: `admin@campusfix.local`
- Password: `ChangeMe123!`

**Change this seeded password before any deployment.** This account is for local demonstration only. Do not expose the demo credentials on a public production deployment.

Register regular accounts through **Create account**. New accounts receive the Student role. The administrator can promote a registered account to Staff using **Users**.

## Demonstration flow

1. Log in as the seeded administrator.
2. Register a separate student account in a private/incognito window or another browser.
3. As the student, submit a complaint (e.g., “Projector not working”, location “Block B, Room 204”).
4. As administrator, open **Users**, change a registered account to Staff, and then assign that staff account to the complaint.
5. As administrator or assigned staff, change the complaint status to In Progress and then Resolved.
6. As the student, verify the status and activity history.
7. Demonstrate search, filters, category management, validation messages and mobile responsiveness.

## Main routes

| Route | Purpose |
|---|---|
| `/` | Landing page |
| `/Account/Register` | Register a student account |
| `/Account/Login` | Sign in |
| `/Dashboard` | Role-aware dashboard |
| `/Complaints` | Search/filter complaints |
| `/Complaints/Create` | Submit a complaint |
| `/Admin/Categories` | Admin category management |
| `/Admin/Users` | Admin role management |
| `/api/complaints` | Authenticated complaint JSON API |
| `/api/complaints/{id}` | Authenticated complaint detail API |
| `/api/complaints/{id}/status` | PATCH complaint status (Admin/Staff) |

### Example API calls

The API is protected by cookie-based authentication. Sign in in the same browser first, then use browser dev tools or an API client with the authenticated session.

```http
GET /api/complaints
GET /api/complaints?status=Pending
GET /api/complaints/1
PATCH /api/complaints/1/status
Content-Type: application/json

{"status":"In Progress"}
```

A student can only view their own complaints. Staff can only view complaints assigned to them. Administrators can access all complaints.

## Architecture

```text
Browser
  |
  v
ASP.NET Core middleware
(Authentication, Authorization, Session, Routing)
  |
  v
MVC Controllers  <---->  Razor Views (HTML forms and UI)
  |
  v
ViewModels + Data Annotations
  |
  v
ApplicationDbContext (EF Core)
  |
  v
MySQL database
```

## Data model

- `ApplicationUser`: Identity account and full name
- `ComplaintCategory`: complaint classification
- `Complaint`: issue details, priority, status, student, assigned staff and dates
- `ComplaintStatusHistory`: status transition audit trail
- ASP.NET Identity tables: users, roles, claims, logins and tokens

## Notes / limitations

- This is a classroom demonstration project. Review security, migrations, logging, email confirmation, rate limiting and deployment secrets before production use.
- Bootstrap and unobtrusive validation libraries are loaded from public CDNs, so internet access is required for those assets. The core server-side validation works without JavaScript.
- The initial schema is created automatically for a fresh local database.
- No real email, online payment, or external college login is configured.
- Use fictitious student information for screenshots and presentations.

## GitHub submission

From the project directory:

```powershell
git init
git add .
git commit -m "Initial CampusFix mini project"
git branch -M main
git remote add origin https://github.com/YOUR-USERNAME/CampusFix.git
git push -u origin main
```

Create the repository on GitHub first and replace `YOUR-USERNAME` with your GitHub username. Never commit passwords or real student data.

## Poster

A2 portrait poster source content is supplied separately as `CampusFix_A2_Poster.pdf`. Replace the screenshot placeholders in the poster with screenshots captured from your own running application before submitting it.
