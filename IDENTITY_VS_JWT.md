# Identity vs JWT - How They Work Together

## 🎯 Quick Answer

**Both are used together!**
- **ASP.NET Core Identity** = User management & password security
- **JWT** = Stateless authentication tokens for API requests

---

## 📊 Visual Breakdown

```
╔═══════════════════════════════════════════════════════════════╗
║                    YOUR AUTHENTICATION SYSTEM                  ║
╚═══════════════════════════════════════════════════════════════╝

┌─────────────────────────────────────────────────────────────┐
│  1. USER REGISTRATION                                        │
└─────────────────────────────────────────────────────────────┘

Client Request:
POST /auth/register
{
  "email": "user@example.com",
  "password": "MyPassword123",
  "userName": "john_doe"
}
           │
           ▼
    ┌──────────────────┐
    │   AuthService    │ ◄─── Uses UserManager<User>
    └──────────────────┘      (from Identity package)
           │
           ▼
┌─────────────────────────────────────────────────────┐
│  ASP.NET Core Identity (UserManager)                │
│  ✅ Validates password requirements                 │
│  ✅ Hashes password with PBKDF2 + salt              │
│  ✅ Stores user in database                         │
│  ✅ Returns IdentityResult                          │
└─────────────────────────────────────────────────────┘
           │
           ▼
    ┌──────────────────┐
    │  TokenService    │ ◄─── Generates JWT
    └──────────────────┘
           │
           ▼
Response:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-02-18T12:00:00Z",
  "user": { ... }
}


┌─────────────────────────────────────────────────────────────┐
│  2. USER LOGIN                                               │
└─────────────────────────────────────────────────────────────┘

Client Request:
POST /auth/login
{
  "email": "user@example.com",
  "password": "MyPassword123"
}
           │
           ▼
    ┌──────────────────┐
    │   AuthService    │
    └──────────────────┘
           │
           ▼
┌─────────────────────────────────────────────────────┐
│  ASP.NET Core Identity (UserManager)                │
│  ✅ Finds user by email                             │
│  ✅ Verifies password hash                          │
│  ✅ Returns user if valid                           │
└─────────────────────────────────────────────────────┘
           │
           ▼
    ┌──────────────────┐
    │  TokenService    │ ◄─── Generates JWT with claims
    └──────────────────┘
           │
           ▼
Response:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-02-18T12:00:00Z",
  "user": { ... }
}


┌─────────────────────────────────────────────────────────────┐
│  3. AUTHENTICATED API REQUEST                                │
└─────────────────────────────────────────────────────────────┘

Client Request:
GET /api/venues
Headers:
  Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
           │
           ▼
┌─────────────────────────────────────────────────────┐
│  JWT Authentication Middleware                      │
│  (Microsoft.AspNetCore.Authentication.JwtBearer)    │
│  ✅ Validates JWT signature                         │
│  ✅ Checks expiration                               │
│  ✅ Extracts claims (userId, email, role)           │
│  ✅ Sets HttpContext.User                           │
└─────────────────────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────────────────┐
│  [Authorize] Attribute                              │
│  ✅ Checks if user is authenticated                 │
│  ✅ Checks role requirements (if specified)         │
└─────────────────────────────────────────────────────┘
           │
           ▼
    ┌──────────────────┐
    │  VenueController │ ◄─── Action executes
    └──────────────────┘
           │
           ▼
Response:
[
  { "id": "...", "name": "Sports Arena", ... }
]
```

---

## 🔧 Technical Implementation

### **Package Roles**

| Package | Purpose | Used For |
|---------|---------|----------|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | User management | Registration, Login, Password hashing |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT validation | Validating tokens on each API request |
| `System.IdentityModel.Tokens.Jwt` | JWT generation | Creating tokens after login |

---

## 💻 Code Flow

### **1. Registration (AuthService.cs)**
```csharp
public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
{
    // IDENTITY: Create user with hashed password
    var user = new User { Email = dto.Email, UserName = dto.UserName };
    var result = await _userManager.CreateAsync(user, dto.Password);
    //                    ↑ Identity handles password hashing

    // JWT: Generate token for immediate login
    var (token, expiresAt) = _tokenService.GenerateAccessToken(user);
    //                        ↑ Custom JWT generation

    return new AuthResponseDto { AccessToken = token, ... };
}
```

### **2. Login (AuthService.cs)**
```csharp
public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
{
    // IDENTITY: Find and validate user
    var user = await _userManager.FindByEmailAsync(dto.Email);
    var isValid = await _userManager.CheckPasswordAsync(user, dto.Password);
    //                   ↑ Identity verifies password hash

    // JWT: Generate token
    var (token, expiresAt) = _tokenService.GenerateAccessToken(user);

    return new AuthResponseDto { AccessToken = token, ... };
}
```

### **3. Token Generation (TokenService.cs)**
```csharp
public (string token, DateTime expiresAt) GenerateAccessToken(User user)
{
    // JWT: Create claims from user data
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role.ToString())
    };

    // JWT: Sign and create token
    var token = new JwtSecurityToken(
        issuer: "Assignment_Example_HU",
        audience: "Assignment_Example_HU_Api",
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(60),
        signingCredentials: creds
    );

    return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
}
```

### **4. Token Validation (Program.cs)**
```csharp
builder.Services.AddJwtAuthentication(configuration);
// ↑ Configures JWT Bearer middleware to validate tokens

// In ServiceCollectionExtensions.JwtAuthentication.cs:
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // ... validation rules
        };
    });
```

---

## 🎯 Summary

| Stage | Identity's Role | JWT's Role |
|-------|----------------|------------|
| **Registration** | Hash & store password | Generate initial token |
| **Login** | Verify password | Generate session token |
| **API Requests** | ❌ Not involved | ✅ Validate token |
| **Authorization** | ❌ Not involved | ✅ Check claims/roles |

---

## ✅ Why This Approach?

### **Identity Package Benefits**
- 🔒 Secure password hashing (PBKDF2)
- 🔑 Built-in user management
- 👥 Role management
- 📧 Email confirmation support
- 🔄 Password reset functionality

### **JWT Benefits**
- 🚀 Stateless (no server-side sessions)
- 📱 Works with mobile apps
- 🌐 Cross-domain authentication
- ⚡ Fast validation
- 📊 Contains user claims

---

## 🔐 Security Flow

```
Registration/Login → Identity validates → JWT generated → Client stores token
                                                              ↓
                                                    Future API requests
                                                              ↓
                                            JWT middleware validates token
                                                              ↓
                                                    Request authorized ✅
```

This is the **industry-standard approach** for modern .NET APIs! 🎉
