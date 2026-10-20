# 🎧 IT Helpdesk Platform

A modern, responsive, and role-based IT Support Ticketing and Knowledge Base management system built with ASP.NET Core MVC. 

This platform streamlines the process of reporting IT issues, requesting hardware/software, and provides a self-service Knowledge Base to reduce the volume of support tickets.

## 🚧 Current Project Status (Milestone 3)

**Important Note for Evaluation:** The project is currently at the **Milestone 3** stage. 
* The full Identity and Authentication system (Login/Register) is scheduled for **Milestone 4**.
* At this current stage, the **Role-Based Access Control is simulated for UI/UX demonstration purposes**. 
* To preview the custom navigation bars, colors, and layouts for each specific user type, you can manually navigate to the following routes:
  * **Guest (Default):** `/Home/GuestDashboard`
  * **User:** `/Home/UserDashboard`
  * **IT Agent:** `/Home/AgentDashboard`
  * **Administrator:** `/Home/AdministratorDashboard`

### ⚠️ Known Limitations (To be addressed in Milestone 4)
Because the full **Identity & Authentication** system is scheduled for the next milestone, the following temporary behaviors are in place:
* **Simulated Sessions:** Since there is no "Logged In" user yet, the **My Profile** and **Ticket Submission** pages use placeholder logic (defaulting to User ID 1) to demonstrate database connectivity.
* **Open Navigation:** Route protection (Authorization) is not yet enforced. You can manually navigate between all dashboards using the URLs provided below.
* **Audit Trails:** Ticket comments and status changes do not yet capture the "Current User," as a security context does not exist.

## ⚙️ Prerequisites

To run this project locally, you will need the following tools installed on your machine:

* **IDE:** [Visual Studio Community 2026](https://visualstudio.microsoft.com/)
* **Framework:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## 👥 User Roles & Capabilities (UI/UX Implemented)

The application features a strict Role-Based Access Control (RBAC) system. The UI, dashboards, and available actions are tailored for each role:

### 1. Guest (Unauthenticated Visitor)
* **Dashboard:** Neutral/Gray Theme
* **Capabilities:**
  * View the Guest Landing Page.
  * Browse and read public guides in the **Knowledge Base** without needing an account.
  * Access the **Sign In** portal to authenticate.

### 2. User (Standard Employee)
* **Dashboard:** Blue Theme (Support Portal)
* **Capabilities:**
  * **Submit Tickets:** Request hardware, software, or report IT issues.
  * **My Tickets:** View the status, priority, and updates of their own submitted tickets.
  * **Communication:** Add comments/replies to their active tickets.
  * **Knowledge Base:** Full read access to self-service articles.
  * *Restriction:* Cannot view other employees' tickets or access management panels.

### 3. IT Agent (Support Technician)
* **Dashboard:** Green Theme (Agent Dashboard with SLA metrics)
* **Capabilities:**
  * **All Tickets:** View and manage tickets submitted by all users across the company.
  * **Ticket Management:** Update ticket Status (e.g., *Open* -> *In Progress* -> *Resolved*).
  * **Prioritization:** Adjust ticket Priority based on urgency.
  * **Communication:** Provide technical support via comments on tickets.
  * **Knowledge Base:** Read access to share articles with users.
  * *Restriction:* Cannot permanently delete tickets or modify system architecture.

### 4. Administrator (System Admin / IT Manager)
* **Dashboard:** Red Theme (Administrator Platform)
* **Capabilities:**
  * **Full System Control:** Access to the overarching management dashboard.
  * **Users:** Manage staff, create new accounts, and assign roles.
  * **Departments:** Full CRUD (Create, Read, Update, Delete) operations for company departments.
  * **Categories:** Manage issue classifications for the ticketing system.
  * **Knowledge Articles:** Full CRUD operations to populate and format the Knowledge Base.
  * **Tickets:** Complete oversight of all tickets, including the ability to permanently delete spam or invalid requests.
  * **All Tickets:** View and manage tickets submitted by all users across the company.
  * **Ticket Management:** Update ticket Status (e.g., *Open* -> *In Progress* -> *Resolved*).

## 🚀 Getting Started

1. Clone the repository to your local machine.
2. Open the solution file (`IT Helpdesk Platform.slnx`) in **Visual Studio 2026**.
3. Build and run the project (F5).
4. (Optional) Open the provided `IT Helpdesk Database.mwb` in **MySQL Workbench** or `Database Diagram.png` to see the database schema and visual tables diagram.

## 🛠️ Troubleshooting (For Evaluators)

If you encounter database-related errors during runtime (e.g., missing tables, "Pending Model Changes", or Entity Framework exceptions), your local database schema is likely out of sync with the C# models. 

You can fix this using the **Package Manager Console (PMC)** in Visual Studio (`Tools` -> `NuGet Package Manager` -> `Package Manager Console`):

**Fix 1: Apply existing migrations** Run the following command to update your local database based on the existing migration files:
`Update-Database`

**Fix 2: Generate a new migration (If Fix 1 fails)**
If `Update-Database` doesn't work or says there are no migrations, your local environment needs a fresh snapshot of the models. Run these two commands in order:

`Add-Migration SyncLocalDb`

`Update-Database`

**Note:** For the **My Profile** page to load, ensure at least one user exists in your database with ID 1.