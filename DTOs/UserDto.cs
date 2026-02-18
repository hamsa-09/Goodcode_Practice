using System;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public Role Role { get; set; }
    }
}

