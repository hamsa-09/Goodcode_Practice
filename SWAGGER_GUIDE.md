# Swagger with JWT Authentication - Complete Guide

## ✅ YES! Swagger is Fully Configured and Working!

Your Swagger UI is **already set up** with JWT Bearer authentication support. Here's everything you need to know:

---

## 🎯 What's Already Configured

### ✅ **1. Swagger with JWT Support**
Location: `Extensions/ServiceCollectionExtensions.Swagger.cs`

```csharp
services.AddSwaggerGen(c =>
{
    // API Documentation
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Assignment Example HU API",
        Version = "v1"
    });

    // JWT Bearer Authentication
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(securityRequirement);
});
```

### ✅ **2. Swagger Enabled in Development**
Location: `Program.cs`

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();        // ← Generates OpenAPI JSON
    app.UseSwaggerUI();      // ← Interactive UI
}
```

### ✅ **3. Controllers with Authorization**
Your controllers use `[Authorize]` and `[AllowAnonymous]` attributes:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // ← Requires authentication by default
public class VenuesController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]  // ← Public endpoint
    public async Task<ActionResult<IEnumerable<VenueDto>>> GetAllVenues()

    [HttpPost]  // ← Requires JWT token (inherited from [Authorize])
    public async Task<ActionResult<VenueDto>> CreateVenue(CreateVenueDto dto)
}
```

---

## 🚀 How to Use Swagger with Authentication

### **Step 1: Start Your Application**
```bash
dotnet run
```

### **Step 2: Open Swagger UI**
Navigate to: **https://localhost:5001/swagger** (or http://localhost:5000/swagger)

### **Step 3: Register a User**
1. Find the **`POST /api/Auth/register`** endpoint
2. Click **"Try it out"**
3. Enter user details:
```json
{
  "email": "test@example.com",
  "password": "Test123!",
  "userName": "testuser",
  "role": 0
}
```
4. Click **"Execute"**
5. **Copy the `accessToken`** from the response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-02-18T12:00:00Z",
  "user": { ... }
}
```

### **Step 4: Authorize in Swagger**
1. Click the **🔓 Authorize** button (top right)
2. In the popup, enter: `Bearer YOUR_TOKEN_HERE`
   ```
   Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```
3. Click **"Authorize"**
4. Click **"Close"**

### **Step 5: Test Protected Endpoints**
Now you can call any protected endpoint! The 🔓 icon will change to 🔒

Example: Try **`POST /api/Venues`** to create a venue.

---

## 📸 Visual Guide

```
┌─────────────────────────────────────────────────────────────┐
│                    SWAGGER UI INTERFACE                      │
└─────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│  Assignment Example HU API v1                    🔓 Authorize │ ← Click here!
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  Auth                                                         │
│  ▼ POST /api/Auth/register    Register a new user            │
│  ▼ POST /api/Auth/login        Login                         │
│                                                               │
│  Venues                                                       │
│  ▼ GET  /api/Venues           Get all venues    🔓           │ ← Public
│  ▼ POST /api/Venues           Create venue      🔒           │ ← Requires JWT
│  ▼ GET  /api/Venues/{id}      Get venue         🔓           │
│  ▼ PUT  /api/Venues/{id}      Update venue      🔒           │
│  ▼ DELETE /api/Venues/{id}    Delete venue      🔒           │
│                                                               │
│  Courts                                                       │
│  ▼ GET  /api/Courts           Get all courts    🔓           │
│  ▼ POST /api/Courts           Create court      🔒           │
│  ...                                                          │
└──────────────────────────────────────────────────────────────┘

When you click "Authorize":
┌─────────────────────────────────────────────────────────────┐
│  Available authorizations                                    │
├─────────────────────────────────────────────────────────────┤
│  Bearer (http, Bearer)                                       │
│                                                              │
│  Value:                                                      │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...        │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  [Authorize]  [Close]                                        │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔍 Endpoint Authorization Status

Your API endpoints have different authorization requirements:

### **Public Endpoints** (🔓 No JWT Required)
```
GET  /api/Venues              - List all venues
GET  /api/Venues/{id}         - Get specific venue
GET  /api/Courts              - List all courts
GET  /api/Courts/{id}         - Get specific court
POST /api/Auth/register       - Register new user
POST /api/Auth/login          - Login
```

### **Protected Endpoints** (🔒 JWT Required)
```
POST   /api/Venues            - Create venue
PUT    /api/Venues/{id}       - Update venue
DELETE /api/Venues/{id}       - Delete venue
POST   /api/Courts            - Create court
PUT    /api/Courts/{id}       - Update court
DELETE /api/Courts/{id}       - Delete court
POST   /api/Games             - Create game
... (all other POST/PUT/DELETE operations)
```

---

## 🧪 Testing Workflow

### **Scenario 1: Public Access**
```
1. Open Swagger
2. Try GET /api/Venues
3. ✅ Works without authentication!
```

### **Scenario 2: Protected Access (Without Token)**
```
1. Open Swagger
2. Try POST /api/Venues (without authorization)
3. ❌ Returns 401 Unauthorized
```

### **Scenario 3: Protected Access (With Token)**
```
1. POST /api/Auth/register → Get token
2. Click "Authorize" → Enter token
3. Try POST /api/Venues
4. ✅ Works! Venue created
```

---

## 🎨 Swagger Features You Have

### ✅ **1. Interactive API Documentation**
- All endpoints listed with descriptions
- Request/response schemas
- Example values

### ✅ **2. Try It Out**
- Execute API calls directly from browser
- See real responses
- No need for Postman!

### ✅ **3. JWT Authentication**
- Authorize button for token input
- Automatic Bearer header injection
- Lock icons showing protected endpoints

### ✅ **4. Request/Response Examples**
```json
// Request Body Example
{
  "name": "Sports Arena",
  "location": "Downtown",
  "description": "Premier sports facility"
}

// Response Example
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Sports Arena",
  "location": "Downtown",
  "ownerName": "testuser",
  "createdAt": "2026-02-18T05:48:17Z"
}
```

### ✅ **5. Validation Errors**
FluentValidation errors are shown in responses:
```json
{
  "errors": {
    "Name": ["Name is required"],
    "Location": ["Location must be between 3 and 200 characters"]
  }
}
```

---

## 🔧 Advanced Swagger Configuration

Your current setup includes:

### **Security Scheme**
```csharp
Type: HTTP
Scheme: bearer
BearerFormat: JWT
Header: Authorization
```

### **Security Requirement**
Applied globally to all endpoints (unless `[AllowAnonymous]` is used)

---

## 📊 What Swagger Shows for Each Endpoint

### **Example: POST /api/Venues**

```
POST /api/Venues
Create a new venue

🔒 Requires: Bearer Token

Parameters:
  (none)

Request Body (application/json):
  {
    "name": "string",
    "location": "string",
    "description": "string"
  }

Responses:
  200 - Success
    {
      "id": "guid",
      "name": "string",
      "location": "string",
      ...
    }

  400 - Bad Request (Validation Error)
  401 - Unauthorized (No/Invalid Token)
  403 - Forbidden (Insufficient Permissions)
```

---

## 🚨 Common Issues & Solutions

### **Issue 1: "401 Unauthorized" on Protected Endpoints**
**Solution**:
1. Register/Login to get a token
2. Click "Authorize" button
3. Enter: `Bearer YOUR_TOKEN`
4. Try again

### **Issue 2: Token Expired**
**Solution**:
1. Login again to get a new token
2. Update authorization with new token

### **Issue 3: Swagger Not Loading**
**Solution**:
1. Check you're in Development mode
2. Verify URL: `/swagger` (not `/swagger/index.html`)
3. Check console for errors

### **Issue 4: "Authorize" Button Not Showing**
**Solution**:
- Already configured! Should be visible at top-right
- If missing, check `ServiceCollectionExtensions.Swagger.cs`

---

## 🎯 Quick Reference

| Action | Steps |
|--------|-------|
| **Access Swagger** | Navigate to `https://localhost:5001/swagger` |
| **Get Token** | POST `/api/Auth/register` or `/api/Auth/login` |
| **Authorize** | Click 🔓 → Enter `Bearer TOKEN` → Authorize |
| **Test Public Endpoint** | Try any GET endpoint (no auth needed) |
| **Test Protected Endpoint** | Authorize first, then try POST/PUT/DELETE |
| **Logout** | Click 🔒 → Logout |

---

## ✅ Summary

### **Your Swagger Setup Includes:**
- ✅ Interactive API documentation
- ✅ JWT Bearer authentication support
- ✅ "Authorize" button for token input
- ✅ Lock icons showing protected endpoints
- ✅ Try-it-out functionality
- ✅ Request/response examples
- ✅ Validation error display
- ✅ All your endpoints documented

### **Everything Works!**
1. **Authentication**: ✅ JWT tokens via `/api/Auth`
2. **Authorization**: ✅ `[Authorize]` and `[AllowAnonymous]` attributes
3. **Swagger UI**: ✅ Fully configured with Bearer auth
4. **All Endpoints**: ✅ Documented and testable

---

## 🚀 Try It Now!

```bash
# Start the application
dotnet run

# Open browser to:
https://localhost:5001/swagger

# Follow the steps above to test!
```

**Your Swagger is production-ready and fully functional!** 🎉
