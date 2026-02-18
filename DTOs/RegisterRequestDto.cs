using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.DTOs
{
    public class RegisterRequestDto
    {
        public string Email { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public Role Role { get; set; } = Role.User;
    }
}

