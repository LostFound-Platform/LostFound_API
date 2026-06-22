# 🎓 Campus Lost & Found API

A modern ASP.NET Core Web API powering a university-focused Lost & Found platform.

Built to help students report, discover, verify, and recover lost items through real-time communication, intelligent matching, and secure claim verification.

---

## 🚀 Features

### Authentication & Authorization
- JWT Authentication
- Email Verification
- Secure User Registration & Login

### Lost & Found Management
- Create Lost Item Posts
- Create Found Item Posts
- Categorize Posts
- Tag & Search Items
- Post Status Tracking

### Smart Matching System
- Match Lost Items with Found Items
- Similarity Scoring
- Match Approval Workflow
- Recovery Tracking

### Real-Time Communication
- SignalR Real-Time Messaging
- Chat Between Finder and Owner
- Instant email notifications

### Claim Verification
- Ownership Verification Process
- Pickup Request Workflow
- Holder Confirmation
- Recovery Status Management

### Notifications
- In-Web Notifications
- Real-Time Updates
- Match Alerts
- Pickup Request Alerts

---

## 🛠 Tech Stack

### Backend
- ASP.NET Core 8
- C#
- Entity Framework Core
- SQL Server
- SignalR

### Authentication
- JWT Bearer Authentication
- ASP.NET Identity

### Database
- SQL Server

### Real-Time Communication
- SignalR Hub

### Email Services
- SMTP Email Sender

### DevOps
- Git
- GitHub
- GitHub Actions (CI/CD)
- Azure App Service

---

## 📂 Solution Structure

```text
LostFound_API
│
├── LostFound_API/           → API Layer
│   ├── Controllers/
│   ├── DTOs/
│   ├── Program.cs
│   └── appsettings.json
│
├── ObjectBusiness/          → Domain Models & DbContext
│   ├── FBLADbContext.cs
│   ├── Users.cs
│   ├── Posts.cs
│   ├── Match.cs
│   ├── Chat.cs
│   └── ...
│
├── DataAccess/              → Data Access Layer
│   ├── UsersDAO.cs
│   ├── PostDAO.cs
│   ├── MatchDAO.cs
│   └── ...
│
├── Repository/              → Business/Data Repository Layer
│   ├── Interfaces
│   ├── Implementations
│   └── Repository Pattern
│
├── Services/                → External Services
│   ├── EmailSender.cs
│   └── HolderReminderService.cs
│
└── SignalRLayer/            → Real-Time Communication
    └── SystemHub.cs
```

---

## 🏗 Architecture

This project follows a layered architecture:

```text
Controller
     │
     ▼
Repository
     │
     ▼
DAO
     │
     ▼
Entity Framework Core
     │
     ▼
SQL Server
```

### Layers

#### Controllers
Handle HTTP requests and responses.

#### Repository
Contains business rules and application logic.

#### DAO
Handles direct database interactions.

#### ObjectBusiness
Contains entities and DbContext.

#### Services
External services such as email sending and scheduled reminders.

#### SignalR Layer
Handles real-time communication between users.

---

## 📊 Database Entities

### User Management
- Users
- Roles
- VerificationCode

### Lost & Found
- Posts
- CategoryPost

### Matching
- Match

### Communication
- Chat
- MessageChat

### Recovery Workflow
- PickupRequest
- TransferRequest
- StatusRequest

### Notifications
- Notifications

### Academic
- Student

---

## 🔄 Main Workflows

### Lost Item Recovery

```text
Student loses item
       │
       ▼
Create Lost Post
       │
       ▼
System Searches Matches
       │
       ▼
Potential Match Found
       │
       ▼
Chat Between Users
       │
       ▼
Verification Process
       │
       ▼
Pickup Request
       │
       ▼
Item Recovered
```

---

## ⚡ API Modules

### Authentication

```http
POST /api/auth/register
POST /api/auth/login
POST /api/auth/verify-email
```

### Users

```http
GET    /api/users
GET    /api/users/{id}
PUT    /api/users/{id}
DELETE /api/users/{id}
```

### Posts

```http
GET    /api/post
POST   /api/post
PUT    /api/post
DELETE /api/post
```

### Matching

```http
GET    /api/match
POST   /api/match
```

### Messaging

```http
GET    /api/messagechat
POST   /api/messagechat
```

### Notifications

```http
GET    /api/notification
```

### Pickup Requests

```http
POST   /api/pickuprequest
GET    /api/pickuprequest
```

---

## ⚙️ Local Development Setup

### Clone Repository

```bash
git clone https://github.com/yourusername/CampusLostFoundAPI.git
```

### Navigate to Project

```bash
cd CampusLostFoundAPI
```

### Restore Packages

```bash
dotnet restore
```

### Update Database

```bash
dotnet ef database update
```

### Run Application

```bash
dotnet run
```

---

## 🔐 Configuration

`appsettings.json`.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "MyConnection": "Data Source=DESKTOP-A6BFNON\\ANSQLSERVER;Database=FBLADB;User Id=sa;Password=Baoan123.cntt;TrustServerCertificate=true;Encrypt=false;"
    //"MyConnection": "Data Source=my-sqlserver.cnoi0okmult3.ap-southeast-2.rds.amazonaws.com,1433;Database=FBLADB;User Id=admin;Password=Baoan123.cntt;TrustServerCertificate=true;Encrypt=false;"
  },
  "AzureSignalR": {
    "ConnectionString": "Endpoint=https://lostandfound-signalr.service.signalr.net;AccessKey=5H02R43oOOa5BOEnT0vq477WDuiU2aQYtcvs53CGsEoKqzgxdF93JQQJ99CAAC1i4TkXJ3w3AAAAASRSY1Xq;Version=1.0;"
  }, // Config to use Azure SignalR service
  "JwtConfig": {
    //"Issuer": "https://localhost:44318/",
    "Issuer": "https://lost-and-found-cqade7hfbjgvcbdq.centralus-01.azurewebsites.net/",
    //"Audience": "http://localhost:5173/",
    "Audience": "https://back2me.vercel.app/",
    "Key": "4d94bf39cf0365286d7bc29a6e34a929c3621a37763160fa7652793a2e1650a817fac5eeccd849e033ddb55061ee00481b9e2df91e6fc1a61c2d36cdf3370f88",
    "TokenValidityMins": 720 // 12 hours
  }
}
```

---

## 🧪 Build

```bash
dotnet build
```

---

## ✅ Run Tests

```bash
dotnet test
```

---

## 🚀 CI/CD

GitHub Actions automatically:

- Restore Dependencies
- Build Solution
- Run Unit Tests
- Validate Pull Requests

Example workflow:

```yaml
Restore Dependencies
Build Solution
Run Tests
```

---

## 📈 Future Enhancements

- AI Image Matching
- OCR Text Recognition
- Mobile Application
- QR Code Recovery System
- Campus Admin Dashboard
- Analytics & Reporting
- Multi-Campus Support
- Azure Blob Storage Integration

---

## 🤝 Contributing

We welcome passionate students, builders, and aspiring contributors who want to grow with the project.

To maintain quality and ensure the right fit, contributors join through a structured pathway rather than direct public pull requests.

### Contributor Pathway

**Application Form → Conversation / Interview → Trial Task or Internship Stage → Contributor → Lead**

### Process Overview

1. **Application Form**
   Submit the application form to express your interest and share your background, skills, and preferred role.

2. **Conversation / Interview**
   Shortlisted applicants will be invited to a conversation or interview to discuss experience, goals, communication, and team fit.

3. **Trial Task / Internship Stage**
   Applicants may complete a trial task or participate in a short internship-style evaluation period to demonstrate technical ability, collaboration, and ownership.

4. **Contributor**
   Candidates who successfully complete the evaluation stage may join the project as contributors.

5. **Lead**
   Contributors who consistently show strong performance, initiative, communication, and impact may be considered for lead roles based on their level and readiness.

> **Note:** We currently do not accept direct unsolicited pull requests. If you’d like to contribute, please start with the application process.

### If you are already a contributor and want to contribute to this ASP.NET Core Web API project:

1. Fork Repository
2. Create Feature Branch

```bash
git checkout -b feature/new-feature
```

3. Commit Changes

```bash
git commit -m "feat: add new feature"
```

4. Push Branch

```bash
git push origin feature/new-feature
```

5. Open Pull Request

---

## 👨‍💻 Author

**Quoc Bao An Nguyen**

Software Engineering Student

- ASP.NET Core
- SQL Server
- Azure
- React
- SignalR
- AI-Powered Applications

GitHub: [https://github.com/JasonPG2007](https://github.com/JasonPG2007)

---

⭐ If you found this project useful, consider giving it a star.
