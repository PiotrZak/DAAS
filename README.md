# DAAS - Document Access Approval System

A modern .NET Core REST API for managing document access requests with CQRS pattern, background notifications, and Docker support.

## 🚀 Quick Start with Docker

### Build and run with Docker Compose:
```bash
docker-compose up --build
```

### API will be available at: http://localhost:5000

## 🧪 Test the CQRS Implementation

### 1. Get all users:
```bash
curl -X GET "http://localhost:5000/api/Users"
```

### 2. Get all documents:
```bash
curl -X GET "http://localhost:5000/api/Documents"
```

### 3. Create an access request (CQRS Command):
```bash
curl -X POST "http://localhost:5000/api/AccessRequests?userId=1" \
  -H "Content-Type: application/json" \
  -d '{
    "documentId": 1,
    "reason": "Testing CQRS implementation",
    "accessType": 0
  }'
```

### 4. Approve the request (triggers background notification):
```bash
curl -X PUT "http://localhost:5000/api/AccessRequests/1/decision?approverId=3" \
  -H "Content-Type: application/json" \
  -d '{
    "isApproved": true,
    "comment": "Approved via CQRS pattern"
  }'
```

### 5. Check pending requests (CQRS Query):
```bash
curl -X GET "http://localhost:5000/api/AccessRequests?pendingOnly=true"
```

## ✅ What's Been Implemented

- **Primary Constructors** - All classes now use C# 12 primary constructor syntax
- **DAAS Naming** - Renamed all projects from DocumentAccessApprovalSystem.* to DAAS.*
- **CQRS Pattern** - Commands and Queries with MediatR
- **Background Events** - Notification system triggers on approval/rejection
- **Docker Support** - Full containerization with multi-stage builds

## 🏗️ Architecture Highlights

- **Commands**: CreateAccessRequest, ApproveAccessRequest
- **Queries**: GetAllUsers, GetAllDocuments, GetAccessRequests, etc.
- **Event Notifications**: AccessRequestDecisionMadeEvent with background handler
- **Clean Architecture**: Domain → Application (CQRS) → Infrastructure → API

## Overview

This system allows users to submit requests for document access, which can then be reviewed and approved/rejected by designated approvers. It includes role-based access control and comprehensive audit tracking.

## Architecture & Design Decisions

### Clean Architecture
The solution follows Clean Architecture with clear separation of concerns:

- **Domain Layer** (`DocumentAccessApprovalSystem.Domain`): Core entities and interfaces
- **Application Layer** (`DocumentAccessApprovalSystem.Application`): Business logic and DTOs
- **Infrastructure Layer** (`DocumentAccessApprovalSystem.Infrastructure`): Data access with EF Core
- **API Layer** (`DocumentAccessApprovalSystem.Api`): REST controllers and configuration

### Key Design Decisions

1. **Entity Framework Core** with SQLite for persistence (easily switchable to other databases)
2. **Repository Pattern** for data access abstraction
3. **DTOs** to decouple domain models from API contracts
4. **Clean separation** between domain logic and infrastructure concerns
5. **Comprehensive unit testing** with Moq and FluentAssertions

## Domain Model

### Entities
- **User**: Represents system users with roles (User, Approver, Admin)
- **Document**: Represents documents that can be accessed
- **AccessRequest**: Represents a user's request to access a document
- **Decision**: Represents an approver's decision on an access request

### User Roles
- **User (0)**: Can submit access requests and view their own requests
- **Approver (1)**: Can approve/reject access requests
- **Admin (2)**: Can approve/reject access requests and manage users

### Access Types
- **Read (0)**: Request read-only access to a document
- **Edit (1)**: Request edit access to a document

### Request Status
- **Pending (0)**: Awaiting approval
- **Approved (1)**: Request has been approved
- **Rejected (2)**: Request has been rejected

## API Endpoints

### Users
- `GET /api/Users` - Get all users
- `GET /api/Users/{id}` - Get user by ID
- `GET /api/Users/by-email/{email}` - Get user by email

### Documents
- `GET /api/Documents` - Get all documents
- `GET /api/Documents/{id}` - Get document by ID

### Access Requests
- `GET /api/AccessRequests` - Get all access requests
- `GET /api/AccessRequests?userId={userId}` - Get requests for a specific user
- `GET /api/AccessRequests?pendingOnly=true` - Get pending requests only
- `POST /api/AccessRequests?userId={userId}` - Create a new access request
- `PUT /api/AccessRequests/{id}/decision?approverId={approverId}` - Approve/reject a request

## Sample API Usage

### 1. Get all users
```bash
curl http://localhost:5000/api/Users
```

### 2. Get all documents
```bash
curl http://localhost:5000/api/Documents
```

### 3. Submit an access request
```bash
curl -X POST "http://localhost:5000/api/AccessRequests?userId=1" \
  -H "Content-Type: application/json" \
  -d '{
    "documentId": 1,
    "reason": "Need to review financial data for quarterly presentation",
    "accessType": 0
  }'
```

### 4. Get pending requests (for approvers)
```bash
curl "http://localhost:5000/api/AccessRequests?pendingOnly=true"
```

### 5. Approve a request
```bash
curl -X PUT "http://localhost:5000/api/AccessRequests/1/decision?approverId=3" \
  -H "Content-Type: application/json" \
  -d '{
    "isApproved": true,
    "comment": "Approved for business analysis"
  }'
```

### 6. Reject a request
```bash
curl -X PUT "http://localhost:5000/api/AccessRequests/2/decision?approverId=3" \
  -H "Content-Type: application/json" \
  -d '{
    "isApproved": false,
    "comment": "Insufficient justification provided"
  }'
```

## Running the Application

### Prerequisites
- .NET 8.0 SDK
- SQLite (for development) or SQL Server (for production)

### Development Setup
1. Clone the repository
2. Navigate to the project directory
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Build the solution:
   ```bash
   dotnet build
   ```
5. Run the API:
   ```bash
   cd src/DocumentAccessApprovalSystem.Api
   dotnet run
   ```
6. Open browser to `http://localhost:5000` for Swagger UI

### Testing
Run all unit tests:
```bash
dotnet test
```

The test suite includes comprehensive tests for:
- Access request creation validation
- Approval workflow logic
- Authorization checks
- Error handling scenarios

## Database

The application uses SQLite for development (stored as `document_approval.db`) and can be configured for other databases in production.

### Sample Data
The application automatically seeds the database with:
- 4 sample users (including users, approvers, and admin)
- 4 sample documents
- 2 sample pending access requests

## Business Logic & Validation

### Access Request Creation
- Validates user and document existence
- Automatically sets status to Pending
- Records timestamp of request

### Decision Making
- Only Approvers and Admins can make decisions
- Cannot change decisions on already-decided requests
- Updates request status based on approval/rejection
- Records decision timestamp and approver information

### Security Considerations
- Role-based authorization for approval actions
- Input validation on all endpoints
- Audit trail for all decisions

## Assumptions & Trade-offs

### Assumptions Made
1. **Simple Authentication**: No complex authentication system implemented for demo purposes
2. **Role Assignment**: User roles are predefined and not changeable via API
3. **Document Management**: Documents are read-only metadata (no actual file storage)
4. **Single Approver**: Each request requires only one approval
5. **No Notifications**: Email/notification system not implemented

### Trade-offs
1. **Simplicity vs Features**: Chose simplicity for demo purposes over advanced features
2. **SQLite vs Production DB**: SQLite for easy setup vs production database
3. **In-Memory Auth**: Simple role-based logic vs comprehensive authentication system
4. **Synchronous Processing**: All operations are synchronous vs asynchronous with queues

## Future Improvements

If given more time, the following enhancements could be implemented:

### Security & Authentication
- JWT token-based authentication
- Role-based authorization middleware
- API key management for different client types

### Features
- Multi-level approval workflows
- Document versioning and access tracking
- Email notifications for request status changes
- Audit logging and reporting
- File upload and storage integration

### Architecture
- CQRS pattern implementation
- Background job processing with Hangfire
- Event sourcing for audit trail
- API versioning strategy

### Operations
- Docker containerization
- CI/CD pipeline setup
- Health checks and monitoring
- API rate limiting
- Comprehensive logging with Serilog

## Project Structure
```
DocumentAccessApprovalSystem/
├── src/
│   ├── DocumentAccessApprovalSystem.Api/          # REST API controllers
│   ├── DocumentAccessApprovalSystem.Application/  # Business logic & DTOs
│   ├── DocumentAccessApprovalSystem.Domain/       # Domain entities & interfaces
│   └── DocumentAccessApprovalSystem.Infrastructure/ # Data access & EF Core
├── tests/
│   └── DocumentAccessApprovalSystem.Tests/        # Unit tests
└── DocumentAccessApprovalSystem.sln               # Solution file
```

This implementation demonstrates clean architecture principles, proper separation of concerns, comprehensive testing, and RESTful API design while maintaining simplicity and clarity.
