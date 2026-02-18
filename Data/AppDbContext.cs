using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Data
{
    public class AppDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<User, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Users and Roles are managed by IdentityDbContext
        public DbSet<Venue> Venues => Set<Venue>();
        public DbSet<Court> Courts => Set<Court>();
        public DbSet<Discount> Discounts => Set<Discount>();
        public DbSet<Slot> Slots => Set<Slot>();
        public DbSet<Game> Games => Set<Game>();
        public DbSet<GamePlayer> GamePlayers => Set<GamePlayer>();
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Refund> Refunds => Set<Refund>();
        public DbSet<Waitlist> Waitlists => Set<Waitlist>();
        public DbSet<Rating> Ratings => Set<Rating>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration - only custom properties (Identity handles base properties)
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Role)
                      .IsRequired();

                entity.Property(u => u.AggregatedRating)
                      .HasPrecision(5, 2);

                entity.Property(u => u.PreferredSports)
                      .HasMaxLength(200);
            });

            // Venue configuration
            modelBuilder.Entity<Venue>(entity =>
            {
                entity.HasKey(v => v.Id);

                entity.Property(v => v.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(v => v.Address)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(v => v.SportsSupported)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(v => v.OwnerId)
                      .IsRequired();

                entity.Property(v => v.ApprovalStatus)
                      .IsRequired();

                entity.HasOne(v => v.Owner)
                      .WithMany(u => u.OwnedVenues)
                      .HasForeignKey(v => v.OwnerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(v => v.OwnerId);
            });

            // Court configuration
            modelBuilder.Entity<Court>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(c => c.VenueId)
                      .IsRequired();

                entity.Property(c => c.SportType)
                      .IsRequired();

                entity.Property(c => c.SlotDurationMinutes)
                      .IsRequired();

                entity.Property(c => c.BasePrice)
                      .IsRequired()
                      .HasPrecision(18, 2);

                entity.Property(c => c.OperatingHours)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasOne(c => c.Venue)
                      .WithMany(v => v.Courts)
                      .HasForeignKey(c => c.VenueId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => c.VenueId);
            });

            // Discount configuration
            modelBuilder.Entity<Discount>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.Property(d => d.Scope)
                      .IsRequired();

                entity.Property(d => d.PercentOff)
                      .IsRequired()
                      .HasPrecision(5, 2);

                entity.Property(d => d.ValidFrom)
                      .IsRequired();

                entity.Property(d => d.ValidTo)
                      .IsRequired();

                entity.HasOne(d => d.Venue)
                      .WithMany(v => v.Discounts)
                      .HasForeignKey(d => d.VenueId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Court)
                      .WithMany(c => c.Discounts)
                      .HasForeignKey(d => d.CourtId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(d => d.VenueId);
                entity.HasIndex(d => d.CourtId);
            });

            // Slot configuration
            modelBuilder.Entity<Slot>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.CourtId)
                      .IsRequired();

                entity.Property(s => s.StartTime)
                      .IsRequired();

                entity.Property(s => s.EndTime)
                      .IsRequired();

                entity.Property(s => s.Price)
                      .IsRequired()
                      .HasPrecision(18, 2);

                entity.Property(s => s.Status)
                      .IsRequired();

                entity.HasOne(s => s.Court)
                      .WithMany(c => c.Slots)
                      .HasForeignKey(s => s.CourtId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.BookedByUser)
                      .WithMany(u => u.BookedSlots)
                      .HasForeignKey(s => s.BookedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(s => s.CourtId);
                entity.HasIndex(s => s.StartTime);
                entity.HasIndex(s => s.Status);
            });

            // Game configuration
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(g => g.Id);

                entity.Property(g => g.SlotId)
                      .IsRequired();

                entity.Property(g => g.Type)
                      .IsRequired();

                entity.Property(g => g.MinPlayers)
                      .IsRequired();

                entity.Property(g => g.MaxPlayers)
                      .IsRequired();

                entity.Property(g => g.Status)
                      .IsRequired();

                entity.Property(g => g.CreatedByUserId)
                      .IsRequired();

                entity.HasOne(g => g.Slot)
                      .WithOne(s => s.Game)
                      .HasForeignKey<Game>(g => g.SlotId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.CreatedByUser)
                      .WithMany(u => u.CreatedGames)
                      .HasForeignKey(g => g.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(g => g.SlotId);
                entity.HasIndex(g => g.CreatedByUserId);
                entity.HasIndex(g => g.Status);
            });

            // GamePlayer configuration
            modelBuilder.Entity<GamePlayer>(entity =>
            {
                entity.HasKey(gp => gp.Id);

                entity.Property(gp => gp.GameId)
                      .IsRequired();

                entity.Property(gp => gp.UserId)
                      .IsRequired();

                entity.HasOne(gp => gp.Game)
                      .WithMany(g => g.Players)
                      .HasForeignKey(gp => gp.GameId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gp => gp.User)
                      .WithMany(u => u.GamePlayers)
                      .HasForeignKey(gp => gp.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(gp => new { gp.GameId, gp.UserId })
                      .IsUnique(); // One user can join a game only once
            });

            // Wallet configuration
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(w => w.Id);

                entity.Property(w => w.UserId)
                      .IsRequired();

                entity.Property(w => w.Balance)
                      .IsRequired()
                      .HasPrecision(18, 2);

                entity.HasOne(w => w.User)
                      .WithOne()
                      .HasForeignKey<Wallet>(w => w.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(w => w.UserId)
                      .IsUnique(); // One wallet per user
            });

            // Transaction configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.WalletId)
                      .IsRequired();

                entity.Property(t => t.UserId)
                      .IsRequired();

                entity.Property(t => t.Type)
                      .IsRequired();

                entity.Property(t => t.Amount)
                      .IsRequired()
                      .HasPrecision(18, 2);

                entity.Property(t => t.BalanceBefore)
                      .IsRequired()
                      .HasPrecision(18, 2);

                entity.Property(t => t.BalanceAfter)
                      .IsRequired()
                      .HasPrecision(18, 2);

                entity.Property(t => t.Status)
                      .IsRequired();

                entity.HasOne(t => t.Wallet)
                      .WithMany(w => w.Transactions)
                      .HasForeignKey(t => t.WalletId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.User)
                      .WithMany()
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.RelatedSlot)
                      .WithMany()
                      .HasForeignKey(t => t.RelatedSlotId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.RelatedGame)
                      .WithMany()
                      .HasForeignKey(t => t.RelatedGameId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(t => t.WalletId);
                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.ReferenceId); // For idempotency checks
                entity.HasIndex(t => t.CreatedAt);
            });

            // Refund configuration
            modelBuilder.Entity<Refund>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.TransactionId)
                      .IsRequired();

                entity.Property(r => r.SlotId)
                      .IsRequired();

                entity.Property(r => r.UserId)
                      .IsRequired();

                entity.Property(r => r.OriginalAmount)
                      .IsRequired()
                      .HasPrecision(18, 2);

                entity.Property(r => r.RefundAmount)
                      .IsRequired()
                      .HasPrecision(18, 2);

                entity.Property(r => r.RefundPercentage)
                      .IsRequired()
                      .HasPrecision(5, 2);

                entity.Property(r => r.Status)
                      .IsRequired();

                entity.Property(r => r.Reason)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.HasOne(r => r.Transaction)
                      .WithMany()
                      .HasForeignKey(r => r.TransactionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Slot)
                      .WithMany()
                      .HasForeignKey(r => r.SlotId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(r => r.TransactionId);
                entity.HasIndex(r => r.SlotId);
                entity.HasIndex(r => r.UserId);
                entity.HasIndex(r => r.ReferenceId); // For idempotency checks
                entity.HasIndex(r => r.Status);
            });

            // Waitlist configuration
            modelBuilder.Entity<Waitlist>(entity =>
            {
                entity.HasKey(w => w.Id);

                entity.Property(w => w.GameId)
                      .IsRequired();

                entity.Property(w => w.UserId)
                      .IsRequired();

                entity.Property(w => w.PlayerRating)
                      .IsRequired()
                      .HasPrecision(5, 2);

                entity.Property(w => w.Priority)
                      .IsRequired();

                entity.HasOne(w => w.Game)
                      .WithMany(g => g.Waitlist)
                      .HasForeignKey(w => w.GameId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.User)
                      .WithMany(u => u.Waitlists)
                      .HasForeignKey(w => w.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(w => new { w.GameId, w.UserId })
                      .IsUnique(); // One user per game in waitlist

                entity.HasIndex(w => w.GameId);
                entity.HasIndex(w => new { w.GameId, w.Priority }); // For sorting
            });

            // Rating configuration
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Type)
                      .IsRequired();

                entity.Property(r => r.RatedById)
                      .IsRequired();

                entity.Property(r => r.Score)
                      .IsRequired();

                entity.Property(r => r.Comment)
                      .HasMaxLength(1000);

                entity.HasOne(r => r.RatedBy)
                      .WithMany(u => u.RatingsGiven)
                      .HasForeignKey(r => r.RatedById)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Venue)
                      .WithMany(v => v.Ratings)
                      .HasForeignKey(r => r.VenueId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Court)
                      .WithMany(c => c.Ratings)
                      .HasForeignKey(r => r.CourtId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Player)
                      .WithMany(u => u.RatingsReceived)
                      .HasForeignKey(r => r.PlayerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Game)
                      .WithMany(g => g.Ratings)
                      .HasForeignKey(r => r.GameId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Unique constraint: One rating per user per game per type
                entity.HasIndex(r => new { r.RatedById, r.GameId, r.Type })
                      .IsUnique()
                      .HasFilter("\"GameId\" IS NOT NULL");

                entity.HasIndex(r => r.VenueId);
                entity.HasIndex(r => r.CourtId);
                entity.HasIndex(r => r.PlayerId);
                entity.HasIndex(r => r.GameId);
            });
        }
    }
}
