using System;
using System.Collections.Generic;

namespace Assignment_Example_HU.DTOs
{
    public class WaitlistDto
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = default!;
        public decimal PlayerRating { get; set; }
        public int Priority { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class JoinWaitlistDto
    {
        public Guid GameId { get; set; }
    }

    public class InviteFromWaitlistDto
    {
        public Guid GameId { get; set; }
        public Guid UserId { get; set; }
    }
}
