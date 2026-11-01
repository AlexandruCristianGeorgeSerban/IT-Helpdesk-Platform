# Intelligent IT Helpdesk Platform

![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core%20MVC-10.0-512BD4?logo=dotnet&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-ORM-512BD4?logo=dotnet&logoColor=white)
![ML.NET](https://img.shields.io/badge/ML.NET-SdcaMaximumEntropy-0078D4?logo=microsoft&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-WebSockets-2B8A3E?logo=dotnet&logoColor=white)
![Bootstrap 5](https://img.shields.io/badge/Bootstrap-5.3-7952B3?logo=bootstrap&logoColor=white)
![License](https://img.shields.io/badge/Project-Academic%20%2F%20Project%20III-blue)

An intelligent, role-based **IT Helpdesk & Incident Management Web Platform** built with **ASP.NET Core MVC** and **Entity Framework Core**. 

Originally developed as a layered N-Tier ticketing baseline (**WAD Milestones 1–4**), this platform is extended during the **Project III (Tools for Program Development)** lifecycle with **Machine Learning ticket classification (ML.NET)**, **TF-IDF & Cosine Similarity semantic recommendations**, **weighted least-loaded agent routing**, **automated SLA background monitoring**, and **real-time SignalR communication**.

---

## Table of Contents
1. [Project Overview & Motivation](#1-project-overview--motivation)
2. [Key Features (Baseline vs. Intelligent Extensions)](#2-key-features-baseline-vs-intelligent-extensions)
3. [System Architecture (N-Tier & Dependency Injection)](#3-system-architecture-n-tier--dependency-injection)
4. [Technology Stack & Development Tools](#4-technology-stack--development-tools)
5. [Project Roadmap & Traceability](#5-project-roadmap--traceability)
6. [Getting Started & Local Setup](#6-getting-started--local-setup)
7. [Performance & Non-Functional Targets](#7-performance--non-functional-targets)
8. [Author](#8-author)

---

## 1. Project Overview & Motivation

Modern IT support teams process a high volume of daily incident reports ranging from hardware malfunctions and network outages to software configuration requests. Traditional helpdesk systems rely on manual triage:
- End-users frequently select the wrong **Category**, **Department**, or **Priority** when submitting a ticket.
- Users submit duplicate tickets for known issues already documented in the **Knowledge Base**.
- Support agents spend significant time rerouting misclassified tickets and diagnosing recurring problems.
- Ticket queues become unbalanced, and approaching **Service Level Agreement (SLA)** deadlines can go unnoticed.

**Objective:** Transform a functional role-based helpdesk baseline into an automated, intelligent support system that classifies incoming tickets, recommends self-service solutions in real time, balances agent workloads, monitors SLA deadlines, and connects users and agents via live WebSockets.

---

## 2. Key Features (Baseline vs. Intelligent Extensions)

### A. Existing Baseline Capabilities (`WAD Milestones 1–4`)
* **Role-Based Access Control & Dedicated Dashboards:** Personalized layouts and views for **Administrator**, **Support Agent**, **Authenticated User (Client)**, and **Guest** (`_AdministratorLayout`, `_AgentLayout`, `_UserLayout`, `_GuestLayout`).
* **End-to-End Ticket Lifecycle:** Users can submit tickets (`SubmitTicket.cshtml`), upload diagnostic attachments (`wwwroot/uploads/tickets/`), track resolution progress (`MyTickets.cshtml`), and post discussion replies (`ViewTicket.cshtml`).
* **Knowledge Base Management:** Support agents and administrators author and publish troubleshooting guides (`KnowledgeArticles`), while users and guests browse and filter articles (`KnowledgeBase/Index.cshtml`).
* **Organizational Administration:** Full CRUD management for users, custom profile avatars (`wwwroot/uploads/avatars/`), support departments, and ticket categories.

### B. New Intelligent Modules (`Project III Scope`)
1. **Automated Ticket Classification & Prioritization (`ML.NET`):**
   * Uses Microsoft ML.NET (`FeaturizeText` + `SdcaMaximumEntropy` multiclass trainer) via `TicketPredictionService` to automatically predict the **Category**, **Department**, and **Priority** (`Low`, `Medium`, `High`) from the ticket's title and description.
   * Supports both one-click **Auto-Classify with AI** pre-filling in the browser and server-side fallback when fields are left on *Auto-Detect*.
2. **Semantic Recommendation Engine (`TF-IDF & Cosine Similarity`):**
   * **User Self-Service Assistant (`SubmitTicket.cshtml`):** As the user types an issue description, debounced AJAX requests compute TF-IDF vectors and Cosine Similarity against published `KnowledgeArticles`, displaying the top 3 matching guides before ticket creation.
   * **Agent Resolution Assistant (`ViewTicket.cshtml`):** Automatically surfaces the top 3 most similar historical **Resolved Tickets** alongside similarity percentages to accelerate root-cause diagnosis.
3. **Smart Agent Workload Balancing & SLA Escalation (`TicketService` & `IHostedService`):**
   * **Weighted Least-Loaded Routing:** Automatically assigns new tickets to the available department agent with the lowest active workload score:
     $$\text{WorkloadScore}(\text{agent}) = \sum_{t \in \text{ActiveTickets}} w(\text{Priority}_t), \quad w(\text{Low})=1,\; w(\text{Medium})=2,\; w(\text{High})=3$$
   * **SLA Background Worker (`SlaMonitorService`):** Periodically scans open tickets in the background and escalates priority when SLA response windows approach or are breached.
4. **Real-Time Collaboration & Instant Notifications (`ASP.NET Core SignalR`):**
   * Replaces static page refreshes in `ViewTicket.cshtml` with a two-way live chat hub (`TicketHub.cs`) and delivers real-time toast alerts upon ticket assignment, status updates, or SLA escalations.

---

## 3. System Architecture (N-Tier & Dependency Injection)

The application strictly enforces separation of concerns through a 3-tier layered architecture:

```text
IT Helpdesk Platform
├── Controllers/                  # Presentation Layer (MVC & AJAX Endpoints)
│   ├── TicketsController.cs      # Ticket CRUD, GetKbSuggestions, PredictTicketFields
│   ├── KnowledgeBaseController.cs
│   ├── AccountController.cs
│   └── ...
├── Services/                     # Business Logic Layer (Injected via DI)
│   ├── TicketService.cs          # Core ticket logic + Weighted Least-Loaded Routing
│   ├── RecommendationService.cs  # Custom TF-IDF & Cosine Similarity NLP Engine
│   ├── TicketPredictionService.cs# ML.NET SdcaMaximumEntropy Multiclass Classifier
│   ├── SlaMonitorService.cs      # IHostedService Background SLA Escalation Worker
│   └── AuthService.cs            # Cookie Authentication & Role Authorization
├── Repositories/                 # Data Access Layer (Generic Repository & Unit of Work)
│   ├── RepositoryBase.cs
│   ├── RepositoryWrapper.cs      # Unit of Work coordinating all entity repositories
│   └── TicketRepository.cs, KnowledgeBaseRepository.cs, UserRepository.cs...
├── Models/                       # Domain Entities, ViewModels & ML Data Schemas
│   ├── Ticket.cs, User.cs, KnowledgeArticle.cs, Comment.cs, Attachment.cs
│   ├── HelpdeskContext.cs        # EF Core DbContext
│   ├── DbSeeder.cs               # Initial database & ML training data seeder
│   └── ML/TicketPredictionModels.cs
└── Views/                        # Razor Views (.cshtml) & Role-Based Layouts
```

---

## 4. Technology Stack & Development Tools

| Category | Technology / Tool | Role in Project |
| :--- | :--- | :--- |
| **IDE & Debugging** | Visual Studio 2022 / VS Code | C# compilation, Razor authoring, integrated debugging, CPU/memory profiling |
| **Backend Framework** | ASP.NET Core MVC & EF Core | N-Tier web architecture, Dependency Injection, LINQ, ORM data persistence |
| **Machine Learning** | ML.NET (`Microsoft.ML`) | Multiclass text classification (`FeaturizeText` + `SdcaMaximumEntropy`) |
| **NLP & IR Engine** | Custom C# TF-IDF & Cosine Similarity | Tokenization, stop-word removal, logarithmic IDF weighting, semantic ranking |
| **Real-Time & Jobs** | ASP.NET Core SignalR & `IHostedService` | Bi-directional WebSocket ticket chat, toast notifications, SLA monitoring |
| **Frontend UI** | Razor Views, Bootstrap 5, jQuery, LibMan | Responsive dashboards, debounced AJAX calls, client-side validation |
| **Database & CASE** | SQL Server / SQLite & MySQL Workbench | Relational storage, EER schema modeling (`IT Helpdesk Database.mwb`) |
| **Traceability & QA** | Git, GitHub, Draw.io, Postman | Version control, milestones, atomic commits, UML diagrams, API latency checks |

---

## 5. Project Roadmap & Traceability

In accordance with the **Tools for Program Development** evaluation criteria, project evolution is tracked through Git tags, milestones, and atomic commits. All project deliverables and artifacts are archived in the [`Docs/`](Docs/) directory:

- [x] **Baseline (`WAD Milestones 1–4`):** Layered N-Tier architecture, EF Core repositories, role-based dashboards, ticketing CRUD, file attachments, and Knowledge Base.
  - 📄 *Artifacts:* [Requirements Document](Docs/Web%20Application%20Design/Requirements_Document.pdf) · [Database EER Diagram](Docs/Database%20Design/Database%20Diagram.png)
- [x] **Preliminary Report (`Report 0`):** Comparative analysis of modern SDLC toolchains, version control traceability, and project stack selection.
  - 📄 *Artifacts:* [Preliminary Report (PDF)](Docs/Project%203%20Tools/Preliminary%20Report/Preliminary%20Report.pdf) · [Presentation (PPTX)](Docs/Project%203%20Tools/Preliminary%20Report/Preliminary%20Report.pptx)
- [x] **P3 Milestone 1 (`Report 1 – Project Proposal`, Weeks 1–3):** Functional & non-functional specifications, toolchain selection, and UML Use Case modeling (Baseline vs. Project III scope).
  - 📄 *Artifacts:* [Project Proposal Report (PDF)](Docs/Project%203%20Tools/Report%201%20-%20Project%20Proposal/IT_Helpdesk_Report1_Project_Proposal.pdf) · [Presentation (PPTX)](Docs/Project%203%20Tools/Report%201%20-%20Project%20Proposal/IT_Helpdesk_Report1_Project_Proposal.pptx)
- [x] **P3 Milestone 2 (`Report 2 – Minimal Design & Prototype`, Weeks 4–7):**
  - 📄 *Design Artifacts:* [Class & Architecture Diagram](Docs/Project%203%20Tools/Report%202%20%20-%20Intermediary/Class%20Diagram%20and%20Architecture.png) · [Prototype Sequence Diagram](Docs/Project%203%20Tools/Report%202%20%20-%20Intermediary/Prototype%20Sequence%20Diagram.png)
  - UML Class/Component Diagram and 3-Phase UML Sequence Diagram.
  - Prototype implementation of `RecommendationService` (TF-IDF & Cosine Similarity).
  - Prototype implementation of `TicketPredictionService` (ML.NET auto-classification).
  - Prototype implementation of Weighted Least-Loaded Agent Routing in `TicketService`.
- [ ] **P3 Milestone 3 (`Final Report – Complete System & Demo`, Weeks 8–11):**
  - Full integration of `TicketHub` (ASP.NET Core SignalR live chat & real-time toast alerts).
  - Full integration of `SlaMonitorService` (`IHostedService` background SLA worker).
  - End-to-end performance benchmarking and final live demonstration.

---

## 6. Getting Started & Local Setup

### Prerequisites
* [.NET SDK 10.0+](https://dotnet.microsoft.com/download/dotnet/10.0)
* Visual Studio 2022 (with *ASP.NET and web development* workload) or Visual Studio Code
* SQL Server LocalDB / SQLite / MySQL (configured via `appsettings.json`)

### Installation Steps

1. **Clone the repository:**
   ```bash
   git clone https://github.com/AlexandruCristianGeorgeSerban/IT-Helpdesk-Platform.git
   cd IT-Helpdesk-Platform
   ```

2. **Restore NuGet & client-side packages:**
   ```bash
   dotnet restore
   ```

3. **Configure the database connection:**
   Check the connection string in `appsettings.json` (`DefaultConnection`). On first startup, `DbSeeder.cs` automatically initializes the schema, default roles (*Admin, Agent, User*), sample Knowledge Base articles, and historical tickets for ML.NET training.

4. **Build and run the application:**
   ```bash
   dotnet run
   ```
   Open `https://localhost:7028` (or the port configured in `Properties/launchSettings.json`) in your browser.

---

## 7. Performance & Non-Functional Targets

* **ML.NET Inference Latency:** `< 100 ms` per ticket prediction request (achieved `~5–15 ms` via in-memory `ITransformer` singleton).
* **TF-IDF Similarity Calculation:** `< 100 ms` per debounced AJAX keystroke query (achieved `~4–18 ms`).
* **SignalR Real-Time Delivery:** `< 50 ms` WebSocket message and alert latency on the local network.
* **Architectural Modularity:** 100% adherence to the Repository + Service Layer pattern via constructor Dependency Injection.

---

## 8. Author

* **[Șerban Alexandru-Cristian-George](https://github.com/AlexandruCristianGeorgeSerban)**
