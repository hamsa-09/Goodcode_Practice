using System;
using Assignment_Example_HU.Enums;

namespace Assignment_Example_HU.DTOs
{
    public class VenueDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string SportsSupported { get; set; } = default!;
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; } = default!;
        public ApprovalStatus ApprovalStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateVenueDto
    {
        public string Name { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string SportsSupported { get; set; } = default!;
    }

    public class UpdateVenueDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? SportsSupported { get; set; }
    }

    public class UpdateVenueApprovalDto
    {
        public ApprovalStatus ApprovalStatus { get; set; }
    }
}
