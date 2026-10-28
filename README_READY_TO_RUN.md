# CampusFix — Ready-to-run project package

This copy targets **.NET 10** and uses a local SQLite database so the web application can run without the current XAMPP/MariaDB service. The original uploaded project is not changed by this package.

## Run it
1. Extract the ZIP.
2. Open the extracted `CampusFix` folder in VS Code.
3. Open Terminal → New Terminal in that folder (the folder containing `CampusFix.csproj`).
4. Run:
   ```powershell
   dotnet restore
   dotnet run
   ```
5. Open the HTTP address printed in the terminal. If the browser does not open automatically, use `http://localhost:5080` if that URL is printed by the launch profile.

## Demo login accounts
- **Admin:** `admin@campusfix.local` / `ChangeMe123!`
- **Staff:** `staff@campusfix.local` / `Staff123!`
- **Student:** `student@campusfix.local` / `Student123!`

The database is created automatically as `campusfix.db` in the project directory, and starter roles/categories/demo users are seeded at startup.

## Included in the supplied project
- ASP.NET Core MVC and Razor views
- Student registration and login/logout
- Role-based Student / Staff / Admin access
- Complaint submission, editing, deletion, search and filters
- Unique complaint reference numbers
- Admin category and user management
- Staff assignment and complaint status updates
- Complaint status history and resolution notes
- Dashboard metrics and REST complaint endpoints
- Responsive CSS interface

## Important
- This package switches the database provider to SQLite so you can get the project running now without fixing XAMPP or setting up a MySQL server. MySQL Workbench is a client, not a database server. If your instructor strictly requires MySQL, we can switch back to MySQL once a server is available.
- This package was adapted from the project you provided, but I cannot execute `dotnet build` in this environment because the .NET SDK is not installed here. Run `dotnet restore` and `dotnet build` on your computer; if any build error appears, paste it and I will fix the project files directly.
- Change demo passwords before deployment. Capture genuine screenshots only after the app is running.
