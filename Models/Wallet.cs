using System;
using System.Collections.Generic;

namespace Assignment_Example_HU.Models
{
    public class Wallet
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Balance { get; set; } = 1000;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = default!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
