
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AuthService(
            UserManager<User> userManager,
            ITokenService tokenService,
            IMapper mapper)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email.Trim());
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            var user = new User
            {
                Email = dto.Email.Trim(),
                UserName = dto.UserName.Trim(),
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                user.UserName = user.Email;
            }

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Registration failed: {errors}");
            }

            var (token, expiresAt) = _tokenService.GenerateAccessToken(user);

            return new AuthResponseDto
            {
                AccessToken = token,
                ExpiresAt = expiresAt,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            // Hardcoded Admin Login
            if (dto.Email == "admin@sport.com" && dto.Password == "Admin@123")
            {
                var adminUser = await _userManager.FindByEmailAsync("admin@sport.com");
                if (adminUser == null)
                {
                    adminUser = new User
                    {
                        UserName = "admin",
                        Email = "admin@sport.com",
                        Role = Role.Admin,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _userManager.CreateAsync(adminUser, "Admin@123");
                }

                var (adminToken, adminExpires) = _tokenService.GenerateAccessToken(adminUser);
                return new AuthResponseDto
                {
                    AccessToken = adminToken,
                    ExpiresAt = adminExpires,
                    User = _mapper.Map<UserDto>(adminUser)
                };
            }

            var user = await _userManager.FindByEmailAsync(dto.Email.Trim());
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var (token, expiresAt) = _tokenService.GenerateAccessToken(user);

            return new AuthResponseDto
            {
                AccessToken = token,
                ExpiresAt = expiresAt,
                User = _mapper.Map<UserDto>(user)
            };
        }
    }
}
