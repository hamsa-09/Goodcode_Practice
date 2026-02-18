using System;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Services.Interfaces
{
    public interface ITokenService
    {
        (string token, DateTime expiresAt) GenerateAccessToken(User user);
    }
}

