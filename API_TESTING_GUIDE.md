# Cinema API - Complete Guide & Postman Testing Instructions

## Table of Contents
1. [What Was Built](#what-was-built)
2. [Architecture Overview](#architecture-overview)
3. [Why is the Route `api/Auth`?](#why-is-the-route-apiauth)
4. [Authentication & Authorization System](#authentication--authorization-system)
5. [Complete Postman Testing Guide](#complete-postman-testing-guide)

---

## What Was Built

I've created a complete **Cinema Management System API** with the following features:

### **Models (Database Entities)**
1. **User** - Stores user accounts with roles (User/Admin)
2. **Movie** - Movie information (title, description, genre, duration, etc.)
3. **Theater** - Cinema theaters/rooms with capacity and seat layout
4. **Screening** - Movie showtimes in theaters with pricing
5. **Ticket** - User ticket purchases with seat assignments

### **Controllers (API Endpoints)**
1. **AuthController** (`/api/Auth`) - Registration and login
2. **MovieController** (`/api/Movie`) - Movie management
3. **TheaterController** (`/api/Theater`) - Theater management
4. **ScreeningController** (`/api/Screening`) - Showtime management
5. **TicketController** (`/api/Ticket`) - Ticket purchasing
6. **UserController** (`/api/User`) - User profile and admin user management

### **Security Features**
- **API Key Authentication** - Required for all `/api/*` endpoints
- **JWT Bearer Tokens** - For user authentication
- **Role-Based Authorization** - Admin vs Regular User permissions

---

## Architecture Overview

### **Request Flow**

```
1. Request comes in
   ↓
2. API Key Middleware checks x-api-key header (for /api/* routes)
   ↓
3. Authentication Middleware validates JWT token (if provided)
   ↓
4. Authorization Middleware checks user roles/permissions
   ↓
5. Controller processes request
   ↓
6. Response sent back
```

### **Two-Layer Security System**

1. **API Key Layer** (Line 100-118 in Program.cs)
   - Applies to ALL routes starting with `/api`
   - Checks for `x-api-key` header
   - Valid keys: `dev-api-key-12345` or `test-api-key-67890`
   - **Purpose**: Basic API access control

2. **JWT Token Layer** (for authenticated endpoints)
   - Validates JWT token in `Authorization: Bearer {token}` header
   - Extracts user ID and role from token
   - **Purpose**: User identity and permissions

---

## Why is the Route `api/Auth`?

### **ASP.NET Core Route Convention**

In ASP.NET Core, when you use the `[Route]` attribute:

```csharp
[Route("api/[controller]")]
public class AuthController : ControllerBase
```

The `[controller]` is a **token replacement** that automatically takes the controller name (without "Controller"):
- Controller class: `AuthController`
- Route template: `api/[controller]`
- Result: `api/Auth`

### **Why `/api` prefix?**

1. **API Key Middleware** (Program.cs line 102):
   ```csharp
   if (!context.Request.Path.StartsWithSegments("/api"))
   ```
   - Only routes starting with `/api` require API key
   - Swagger UI and other routes don't need API keys

2. **RESTful Convention**:
   - `/api/Movie` - Movie resources
   - `/api/Auth` - Authentication endpoints
   - Clear separation between API and web pages

### **Could it be different?**

Yes! You could change it:
- `[Route("auth")]` → `/auth` (no API key required!)
- `[Route("v1/authentication")]` → `/v1/authentication`
- But keep `/api` prefix to use the API key protection

---

## Authentication & Authorization System

### **How Authentication Works**

1. **Registration** (`POST /api/Auth/register`)
   - User provides email/username and password
   - Password is hashed with HMACSHA512
   - User stored with default role "User"
   - Returns success message

2. **Login** (`POST /api/Auth/login`)
   - User provides credentials
   - Password verified against hash
   - JWT token generated with:
     - Username (ClaimTypes.Name)
     - User ID (ClaimTypes.NameIdentifier)
     - Role (ClaimTypes.Role)
   - Token expires in 1 hour

3. **Using the Token**
   - Include in header: `Authorization: Bearer {token}`
   - API validates token signature, expiration, issuer
   - Extracts user claims (ID, role)

### **How Authorization Works**

**Policies Defined** (Program.cs lines 48-52):
```csharp
"AdminOnly" - Requires role = "Admin"
"Authenticated" - Requires valid JWT token (any role)
```

**Usage in Controllers**:
```csharp
[Authorize(Policy = "AdminOnly")]  // Only admins
[Authorize(Policy = "Authenticated")]  // Any logged-in user
// No attribute = Public endpoint
```

---

## Complete Postman Testing Guide

### **Prerequisites**
1. Start your API: `dotnet run`
2. Start PostgreSQL: `docker-compose up -d`
3. Run migrations: `dotnet ef database update`
4. API runs on: `http://localhost:5078` (check your launchSettings.json)

---

### **Step 1: Setup Postman Environment**

1. Create a new Environment in Postman
2. Add variables:
   - `base_url`: `http://localhost:5078`
   - `api_key`: `dev-api-key-12345`
   - `jwt_token`: (leave empty, will be set after login)

3. Create a **Collection** named "Cinema API"
4. Add collection-level headers:
   - Key: `x-api-key`, Value: `{{api_key}}`
   - Key: `Content-Type`, Value: `application/json`

---

### **Step 2: Test Public Endpoints (No Auth Required)**

#### **2.1 Get All Movies**
```
Method: GET
URL: {{base_url}}/api/Movie
Headers: 
  - x-api-key: {{api_key}}
```
**Expected**: List of movies (might be empty initially)

#### **2.2 Get All Theaters**
```
Method: GET
URL: {{base_url}}/api/Theater
Headers: 
  - x-api-key: {{api_key}}
```

#### **2.3 Get Screenings**
```
Method: GET
URL: {{base_url}}/api/Screening
Headers: 
  - x-api-key: {{api_key}}
```

---

### **Step 3: User Registration & Login**

#### **3.1 Register a Regular User**
```
Method: POST
URL: {{base_url}}/api/Auth/register
Headers:
  - x-api-key: {{api_key}}
  - Content-Type: application/json
Body (raw JSON):
{
  "email": "user@example.com",
  "password": "password123"
}
```
**Expected Response**: `"User registered successfully."`

#### **3.2 Register an Admin User** (You'll need to manually set role in DB later, or use another admin)
```
Method: POST
URL: {{base_url}}/api/Auth/register
Headers:
  - x-api-key: {{api_key}}
  - Content-Type: application/json
Body:
{
  "email": "admin@example.com",
  "password": "admin123"
}
```

#### **3.3 Login as User**
```
Method: POST
URL: {{base_url}}/api/Auth/login
Headers:
  - x-api-key: {{api_key}}
  - Content-Type: application/json
Body:
{
  "email": "user@example.com",
  "password": "password123"
}
```
**Expected Response**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "user"
}
```

**ACTION**: Copy the `token` value and:
1. Set it in your Postman environment variable `jwt_token`
2. Or manually add `Authorization: Bearer {token}` header to requests

---

### **Step 4: Setup Auth Token Automatically**

Create a **Test Script** in your Login request (Tests tab in Postman):

```javascript
if (pm.response.code === 200) {
    var jsonData = pm.response.json();
    pm.environment.set("jwt_token", jsonData.token);
    console.log("Token saved:", jsonData.token);
}
```

Now after login, the token is automatically saved!

---

### **Step 5: Test Authenticated Endpoints (User)**

#### **5.1 Get Your Profile**
```
Method: GET
URL: {{base_url}}/api/User/profile
Headers:
  - x-api-key: {{api_key}}
  - Authorization: Bearer {{jwt_token}}
```
**Expected**: Your user profile with ticket count

#### **5.2 Get Your Tickets**
```
Method: GET
URL: {{base_url}}/api/Ticket/my-tickets
Headers:
  - x-api-key: {{api_key}}
  - Authorization: Bearer {{jwt_token}}
```
**Expected**: Empty array (no tickets yet)

---

### **Step 6: Admin Operations** (Create Movies, Theaters, Screenings)

**Note**: You need an admin user. To create one:
1. Register a user normally
2. Update database: `UPDATE "Users" SET "Role" = 'Admin' WHERE "Email" = 'admin@example.com';`
3. Login again to get new token with Admin role

#### **6.1 Create a Movie (Admin Only)**
```
Method: POST
URL: {{base_url}}/api/Movie
Headers:
  - x-api-key: {{api_key}}
  - Authorization: Bearer {{admin_jwt_token}}
  - Content-Type: application/json
Body:
{
  "title": "The Matrix",
  "description": "A computer hacker learns about the true nature of reality.",
  "genre": "Sci-Fi",
  "durationMinutes": 136,
  "releaseDate": "1999-03-31T00:00:00Z",
  "director": "Wachowski Brothers",
  "rating": "R"
}
```
**Expected**: Created movie object with ID

#### **6.2 Create a Theater (Admin Only)**
```
Method: POST
URL: {{base_url}}/api/Theater
Headers:
  - x-api-key: {{api_key}}
  - Authorization: Bearer {{admin_jwt_token}}
  - Content-Type: application/json
Body:
{
  "name": "Theater 1",
  "capacity": 50,
  "rows": 5,
  "seatsPerRow": 10
}
```
**Expected**: Created theater object

#### **6.3 Create a Screening (Admin Only)**
```
Method: POST
URL: {{base_url}}/api/Screening
Headers:
  - x-api-key: {{api_key}}
  - Authorization: Bearer {{admin_jwt_token}}
  - Content-Type: application/json
Body:
{
  "movieId": 1,
  "theaterId": 1,
  "showTime": "2024-12-25T18:00:00Z",
  "price": 12.50
}
```
**Expected**: Created screening with availability info

---

### **Step 7: Test Ticket Purchasing (User)**

#### **7.1 Purchase a Ticket**
```
Method: POST
URL: {{base_url}}/api/Ticket/purchase
Headers:
  - x-api-key: {{api_key}}
  - Authorization: Bearer {{jwt_token}}
  - Content-Type: application/json
Body:
{
  "screeningId": 1,
  "seatNumber": "A5"
}
```
**Expected**: Ticket object with details

**Note**: Seat format is `{RowLetter}{SeatNumber}`:
- Row A, Seat 1 = `A1`
- Row B, Seat 10 = `B10`
- Valid for a 5-row, 10-seat theater: A1-A10, B1-B10, C1-C10, D1-D10, E1-E10

#### **7.2 View Your Tickets**
```
Method: GET
URL: {{base_url}}/api/Ticket/my-tickets
Headers:
  - x-api-key: {{api_key}}
  - Authorization: Bearer {{jwt_token}}
```
**Expected**: Array with your purchased ticket

#### **7.3 Cancel a Ticket**
```
Method: POST
URL: {{base_url}}/api/Ticket/1/cancel
Headers:
  - x-api-key: {{api_key}}
  - Authorization: Bearer {{jwt_token}}
```
**Expected**: "Ticket cancelled successfully."

---

### **Step 8: Test Advanced Features**

#### **8.1 Search Movies**
```
Method: GET
URL: {{base_url}}/api/Movie?search=matrix&genre=Sci-Fi
Headers:
  - x-api-key: {{api_key}}
```

#### **8.2 Get Upcoming Screenings**
```
Method: GET
URL: {{base_url}}/api/Screening/upcoming?limit=5
Headers:
  - x-api-key: {{api_key}}
```

#### **8.3 Get Screening Details with Seat Availability**
```
Method: GET
URL: {{base_url}}/api/Screening/1
Headers:
  - x-api-key: {{api_key}}
```
**Expected**: Screening info with `occupiedSeats` array

---

### **Step 9: Test Admin User Management**

#### **9.1 Get All Users (Admin Only)**
```
Method: GET
URL: {{base_url}}/api/User?role=User
Headers:
  - x-api-key: {{api_key}}
  - Authorization: Bearer {{admin_jwt_token}}
```

#### **9.2 Update User Role (Admin Only)**
```
Method: PUT
URL: {{base_url}}/api/User/2/role
Headers:
  - x-api-key: {{api_key}}
  - Authorization: Bearer {{admin_jwt_token}}
  - Content-Type: application/json
Body:
{
  "role": "Admin"
}
```

---

## Postman Collection Structure

Create folders in your collection:

```
Cinema API Collection
├── 🔓 Public Endpoints
│   ├── GET Movies
│   ├── GET Theaters
│   ├── GET Screenings
│   └── GET Screening Details
├── 🔐 Authentication
│   ├── POST Register
│   ├── POST Login
│   └── GET Profile
├── 🎬 Admin - Movies
│   ├── POST Create Movie
│   ├── PUT Update Movie
│   └── DELETE Movie
├── 🎭 Admin - Theaters
│   ├── POST Create Theater
│   └── PUT Update Theater
├── 📅 Admin - Screenings
│   ├── POST Create Screening
│   └── GET Upcoming
├── 🎫 User - Tickets
│   ├── POST Purchase Ticket
│   ├── GET My Tickets
│   └── POST Cancel Ticket
└── 👥 Admin - Users
    ├── GET All Users
    └── PUT Update Role
```

---

## Common Issues & Solutions

### **Issue 1: "API Key is missing or invalid"**
- **Solution**: Add `x-api-key: dev-api-key-12345` header to ALL `/api/*` requests

### **Issue 2: "401 Unauthorized"**
- **Solution**: 
  - Login first to get JWT token
  - Add `Authorization: Bearer {token}` header
  - Check token hasn't expired (1 hour lifetime)

### **Issue 3: "403 Forbidden" on Admin endpoints**
- **Solution**: 
  - User must have `Role = "Admin"` in database
  - Login again to get new token with Admin role

### **Issue 4: "Seat is already taken"**
- **Solution**: 
  - Check available seats first: `GET /api/Screening/{id}`
  - Choose a seat from the available ones

### **Issue 5: "Cannot purchase ticket for a past screening"**
- **Solution**: Create screenings with future dates/times

---

## Quick Test Checklist

- [ ] ✅ API Key works (test any public endpoint)
- [ ] ✅ Register user
- [ ] ✅ Login and get token
- [ ] ✅ View profile (authenticated)
- [ ] ✅ Create movie (admin)
- [ ] ✅ Create theater (admin)
- [ ] ✅ Create screening (admin)
- [ ] ✅ Purchase ticket (user)
- [ ] ✅ View my tickets (user)
- [ ] ✅ Cancel ticket (user)
- [ ] ✅ Get all users (admin)
- [ ] ✅ Update user role (admin)

---

## Additional Notes

### **API Key Protection**
- ALL routes starting with `/api` require API key
- Swagger UI at root (`/`) doesn't require API key
- This is configured in `Program.cs` lines 100-118

### **JWT Token Lifecycle**
- Tokens expire after 1 hour
- User must login again to get new token
- Token contains: username, user ID, and role

### **Role System**
- Default role: `"User"`
- Admin role: `"Admin"`
- Set in database or via Admin endpoint

### **Database Migrations**
Don't forget to run:
```bash
dotnet ef migrations add CinemaInitial
dotnet ef database update
```

---

Happy Testing! 🎬🎫

