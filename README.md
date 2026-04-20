# Project Approval System (PAS)

A secure, web-based Project Approval System that supports **Blind Matching** between students and supervisors. Built with ASP.NET Core MVC, Entity Framework Core, and SQL Server.

---

## Overview

The Project Approval System (PAS) is designed to manage the lifecycle of student coursework project proposals. It enables students to submit project proposals anonymously, allows supervisors to review and express interest in projects without knowing the student's identity, and reveals both parties' identities only after a mutual match is confirmed.

### Key Features

- **Role-Based Access Control** — Three distinct user roles: Student, Supervisor, and Module Leader (Admin), each with their own dashboard and permissions.
- **Blind Matching** — Supervisors browse and review proposals anonymously. Student identity is never exposed during the review phase.
- **Identity Reveal** — Upon match confirmation, both the student and supervisor identities are revealed securely through a dedicated match details page.
- **Proposal Management** — Students can submit, edit, and withdraw proposals. Editing and withdrawal are automatically locked once a match is confirmed.
- **Administrative Oversight** — Module Leaders can monitor all proposals, manage users, oversee matches, and cancel matches if needed.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Framework** | ASP.NET Core MVC (.NET 10) |
| **Language** | C# 12 |
| **ORM** | Entity Framework Core |
| **Database** | Microsoft SQL Server (LocalDB / Express) |
| **Authentication** | ASP.NET Core Identity (Role-based) |
| **Frontend** | Razor Views, Bootstrap 5.3, Bootstrap Icons |
| **Typography** | Google Fonts (Inter) |
| **Testing** | xUnit, Moq |

---

## Project Structure

```
ProjectApprovalSystem/
├── Areas/Identity/Pages/Account/    # Login & Registration (Razor Pages)
├── Controllers/
│   ├── HomeController.cs            # Public landing page
│   ├── StudentController.cs         # Proposal submission & management
│   ├── SupervisorController.cs      # Blind review & match confirmation
│   └── AdminController.cs           # Oversight & user management
├── Data/
│   ├── ApplicationDbContext.cs      # Database schema & relationships
│   └── DbSeeder.cs                  # Seeds roles, research areas & test accounts
├── Interfaces/                      # Service abstractions
├── Models/
│   ├── ApplicationUser.cs           # Custom Identity user
│   ├── Enums.cs                     # ProjectStatus, MatchStatus, UserRole
│   ├── Profiles.cs                  # StudentProfile, SupervisorProfile
│   └── ProjectModels.cs             # ProjectProposal, ResearchArea, MatchRecord
├── Services/
│   ├── MatchingService.cs           # Blind matching & identity reveal logic
│   ├── ProposalService.cs           # Proposal CRUD operations
│   └── ManagementServices.cs        # Admin & user management
├── ViewModels/                      # Data transfer objects for views
├── Views/
│   ├── Student/                     # Dashboard, CreateProposal, Edit, Details
│   ├── Supervisor/                  # Dashboard, BlindReview, MatchDetails
│   ├── Admin/                       # Dashboard, ManageUsers, MatchOversight
│   └── Shared/                      # Layout, LoginPartial
├── wwwroot/                         # Static assets (CSS, JS, Bootstrap)
├── Program.cs                       # App startup & dependency injection
└── appsettings.json                 # Configuration & connection strings
```

---

## Prerequisites

Before running the application, ensure the following are installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (or later)
- **SQL Server LocalDB** (included with Visual Studio) or **SQL Server Express**
- **EF Core CLI Tools** — install with:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## Getting Started

### 1. Clone or Extract the Project
```bash
cd ProjectApprovalSystem
```

### 2. Configure the Database

The default connection string in `appsettings.json` uses LocalDB:
```json
"Server=(localdb)\\mssqllocaldb;Database=ProjectApprovalSystem;Trusted_Connection=True"
```

If you are using **SQL Server Express** instead, change it to:
```json
"Server=.\\SQLEXPRESS;Database=ProjectApprovalSystem;Trusted_Connection=True;TrustServerCertificate=True"
```

### 3. Create the Database
```bash
dotnet ef database update
```

### 4. Run the Application
```bash
dotnet run
```

Open your browser and navigate to: **http://localhost:5194**

---

## Test Accounts

The system automatically seeds the following accounts on first run:

| Role | Email | Password |
|------|-------|----------|
| **Student** | student@pas.com | Student@123 |
| **Supervisor** | supervisor@pas.com | Supervisor@123 |
| **Admin (Module Leader)** | admin@pas.com | Admin@123 |

---

## User Workflows

### Student Flow
1. Register or log in as a Student
2. Navigate to "Submit New" to create a project proposal
3. Fill in the Title, Research Area, Abstract, and Technical Stack
4. Submit the proposal — status becomes **Pending**
5. Track proposal status on the Student Dashboard
6. Once matched, view the assigned supervisor's details

### Supervisor Flow
1. Log in as a Supervisor
2. Set research expertise via "My Expertise"
3. Browse anonymous proposals on the Blind Review Dashboard
4. Click "Express Interest" on a suitable project — status becomes **Under Review**
5. Click "Confirm Match" to trigger the **Identity Reveal**
6. View full student details on the Match Details page

### Admin Flow
1. Log in as a Module Leader
2. View system-wide statistics on the Admin Dashboard
3. Manage users, research areas, and monitor all matches
4. Cancel matches if needed (resets proposal to Pending)

---

## How Blind Matching Works

The system enforces anonymity through a dedicated `ProposalSummaryViewModel` that excludes all student-identifying fields. The database query uses an explicit `.Select()` projection so that student data is never fetched from the database during the anonymous review phase.

Identity is revealed only when:
1. A supervisor confirms a match
2. The backend sets `IsMatched = true` and `Status = Matched`
3. A separate service method (`GetMatchDetailsAsync`) checks the `IsMatched` flag before loading any personal information

---

## Project Status Stages

```
Pending → Under Review → Matched
                ↓
           Withdrawn (terminal)
```

- **Pending** — Proposal submitted, awaiting supervisor interest
- **Under Review** — At least one supervisor has expressed interest
- **Matched** — A supervisor has confirmed the match; identities revealed
- **Withdrawn** — Student withdrew the proposal (only allowed before matching)

---

## Testing

The project includes a test suite in `ProjectApprovalSystem.Tests/`:

```bash
dotnet test
```

### Test Coverage
- **MatchingServiceTests.cs** — Unit tests for express interest, confirm match, cancel match
- **ProposalServiceTests.cs** — Unit tests for proposal creation, update blocking, withdrawal
- **IntegrationTests.cs** — End-to-end flow from submission to identity reveal

---

## Database Tables

| Table | Purpose |
|-------|---------|
| AspNetUsers | User accounts (extended with FullName, IsActive) |
| AspNetRoles | Role definitions (Student, Supervisor, ModuleLeader) |
| StudentProfiles | Student-specific data (StudentId) |
| SupervisorProfiles | Supervisor-specific data (StaffId) |
| ProjectProposals | Project details, status, matching flags |
| ResearchAreas | Research domain categories |
| MatchRecords | Student-supervisor matching history |
| SupervisorExpertise | Supervisor research area preferences |

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| `dotnet-ef` not found | Run `dotnet tool install --global dotnet-ef` |
| Cannot connect to SQL Server | Check `appsettings.json` connection string matches your SQL installation |
| LocalDB not found | Either install LocalDB or switch connection string to `.\SQLEXPRESS` |
| .NET SDK version mismatch | Install the correct .NET SDK version from https://dotnet.microsoft.com/download |

---

## License

This project was developed as part of academic coursework.
