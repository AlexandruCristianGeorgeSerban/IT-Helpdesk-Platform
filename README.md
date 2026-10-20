# 🎧 IT Helpdesk Platform

A modern, responsive, and role-based IT Support Ticketing and Knowledge Base management system built with ASP.NET Core MVC. 

This platform streamlines the process of reporting IT issues, requesting hardware/software, and provides a self-service Knowledge Base to reduce the volume of support tickets.

## 🚀 Current Project Status (Milestone 4 - Final)

The project has successfully reached **Milestone 4**. The full **ASP.NET Core Identity** and Authentication system is now securely integrated.

### ✨ Key Features Implemented in Milestone 4:
* **Robust Authentication & Security:** Fully functional Login, Registration, and session management using ASP.NET Core Identity. Passwords are securely hashed.
* **Role-Based Access Control (RBAC):** Strict route protection using `[Authorize(Roles = "...")]`. Users cannot access unauthorized dashboards or endpoints by typing URLs.
* **Dynamic Database Seeding:** On the first run, the database is automatically populated with Departments, Categories, Roles, Knowledge Articles, realistic Tickets, threaded Comments, and default user accounts.
* **File Management:** Users can upload profile pictures, and users/agents can attach images to ticket comments. The system includes fallback UI elements if files are deleted from the server.
* **Data Integrity:** Eliminated Entity Framework tracking collisions, created safe navigation fallbacks, and handled Null Reference Exceptions for a crash-free experience.

## ⚙️ Prerequisites

To run this project locally, you will need the following tools installed on your machine:

* **IDE:** [Visual Studio Community 2026](https://visualstudio.microsoft.com/)
* **Framework:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## 🔑 Default Test Accounts (Generated via Seeder)

When you first run the application, the database seeder automatically creates the following accounts. You can use these to test the different role-based dashboards:

* **Administrator:** `admin@helpdesk.com` / `Admin123!`
* **IT Agent:** `agent@helpdesk.com` / `Agent123!`
* **Standard User:** `user@helpdesk.com` / `User123!`

## 👥 User Roles & Capabilities

The application features a strict Role-Based Access Control (RBAC) system. The UI, dashboards, and available actions are dynamically tailored for each role:

### 1. Guest (Unauthenticated Visitor)
* View the Guest Landing Page.
* Browse and read public guides in the **Knowledge Base** without needing an account.

### 2. User (Standard Employee)
* **Submit Tickets:** Request hardware, software, or report IT issues.
* **My Tickets:** View the status, priority, and updates of their own submitted tickets.
* **Communication:** Add comments, replies, and file attachments to their active tickets.
* **Profile Management:** Update contact info, preferences, and profile avatar.

### 3. IT Agent (Support Technician)
* **All Tickets:** View, search, and manage tickets submitted by all users across the company.
* **Ticket Management:** Update ticket Status (e.g., *Open* -> *In Progress* -> *Resolved*), Priority, and Assigned Agent.
* **Communication:** Provide technical support via comments and attachments.

### 4. Administrator (System Admin / IT Manager)
* **Full System Control:** Access to the overarching management dashboard.
* **Users Management:** View staff, edit profiles, and manage active accounts.
* **System Architecture:** Full CRUD operations for company **Departments** and ticket **Categories**.
* **Knowledge Articles:** Full CRUD operations to populate and format the Knowledge Base.
* **Tickets:** Complete oversight and deletion rights for all tickets.

## 🚀 Getting Started

1. Clone the repository.
2. Open the solution file (`IT Helpdesk Platform.slnx`) in **Visual Studio 2026**.
3. Open the **Package Manager Console (PMC)** and run: `Update-Database`

## 🛠️ Troubleshooting (For Evaluators)

If you encounter database-related errors during runtime (e.g., missing tables, "Pending Model Changes", or Entity Framework exceptions), your local database schema is likely out of sync with the C# models. 

**⚠️ IMPORTANT FOR EVALUATORS (Previous Milestones):**
If you have previously evaluated this project during **Milestone 2 or 3**, you **must delete the old database** before running this version. The new ASP.NET Core Identity integration drastically changes the database schema and will conflict with the old tables.
1. Open **SQL Server Object Explorer** in Visual Studio.
2. Find the old database, right-click it, and select **Delete** (make sure to check "Close existing connections").

After deleting the old database (or if you are running it for the first time), you can fix sync issues using the **Package Manager Console (PMC)** in Visual Studio (`Tools` -> `NuGet Package Manager` -> `Package Manager Console`):

**Fix 1: Apply existing migrations** Run the following command to update your local database based on the existing migration files and trigger the seeder:
`Update-Database`

**Fix 2: Generate a new migration (If Fix 1 fails)**
If `Update-Database` doesn't work or says there are no migrations, your local environment needs a fresh snapshot of the models. Run these two commands in order:

`Add-Migration SyncLocalDb`

`Update-Database`
