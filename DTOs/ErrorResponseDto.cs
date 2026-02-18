using System;

namespace Assignment_Example_HU.DTOs
{
    public class ErrorResponseDto
    {
        public bool Success { get; set; } = false;
        public string Type { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
