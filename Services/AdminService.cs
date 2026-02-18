using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _dbContext;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly IMapper _mapper;

        public AdminService(
            AppDbContext dbContext,
            ITransactionRepository transactionRepository,
            IVenueRepository venueRepository,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _transactionRepository = transactionRepository;
            _venueRepository = venueRepository;
            _mapper = mapper;
        }

        public async Task<AdminStatsDto> GetDashboardStatsAsync()
        {
            var totalUsers = await _dbContext.Users.CountAsync();
            var totalVenues = await _dbContext.Venues.CountAsync();
            var totalCourts = await _dbContext.Courts.CountAsync();
            var totalBookings = await _dbContext.Slots.CountAsync(s => s.Status == SlotStatus.Booked || s.Status == SlotStatus.Completed);

            var transactions = await _transactionRepository.GetAllAsync();
            var revenue = transactions
                .Where(t => t.Type == TransactionType.Debit && t.Status == TransactionStatus.Completed)
                .Sum(t => t.Amount);

            var refunds = transactions
                .Where(t => t.Type == TransactionType.Credit && t.Status == TransactionStatus.Completed && t.ReferenceId.StartsWith("REFUND_"))
                .Sum(t => t.Amount);

            var popularVenues = await _dbContext.Slots
                .Include(s => s.Court)
                    .ThenInclude(c => c.Venue)
                .Where(s => s.Status == SlotStatus.Booked || s.Status == SlotStatus.Completed)
                .GroupBy(s => new { s.Court.VenueId, s.Court.Venue.Name })
                .Select(g => new VenuePopularityDto
                {
                    VenueId = g.Key.VenueId,
                    VenueName = g.Key.Name,
                    BookingCount = g.Count()
                })
                .OrderByDescending(v => v.BookingCount)
                .Take(5)
                .ToListAsync();

            return new AdminStatsDto
            {
                TotalUsers = totalUsers,
                TotalVenues = totalVenues,
                TotalCourts = totalCourts,
                TotalBookings = totalBookings,
                TotalRevenue = revenue - refunds,
                PopularVenues = popularVenues
            };
        }

        public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync()
        {
            var transactions = await _transactionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }

        public async Task<IEnumerable<VenueDto>> GetPendingVenuesAsync()
        {
            var venues = await _dbContext.Venues
                .Where(v => v.ApprovalStatus == ApprovalStatus.Pending)
                .ToListAsync();
            return _mapper.Map<IEnumerable<VenueDto>>(venues);
        }
    }
}
