using System;

namespace Assignment_Example_HU.DTOs
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = default!;
    }
}

