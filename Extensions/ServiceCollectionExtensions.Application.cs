using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Assignment_Example_HU.BackgroundServices;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services;
using Assignment_Example_HU.Services.Interfaces;

using Assignment_Example_HU.Mappings;

namespace Assignment_Example_HU.Extensions
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IVenueRepository, VenueRepository>();
            services.AddScoped<ICourtRepository, CourtRepository>();
            services.AddScoped<IDiscountRepository, DiscountRepository>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<IGamePlayerRepository, GamePlayerRepository>();
            services.AddScoped<ISlotRepository, SlotRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IRefundRepository, RefundRepository>();
            services.AddScoped<IWaitlistRepository, WaitlistRepository>();
            services.AddScoped<IRatingRepository, RatingRepository>();

            // Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IVenueService, VenueService>();
            services.AddScoped<ICourtService, CourtService>();
            services.AddScoped<IDiscountService, DiscountService>();
            services.AddScoped<IGameService, GameService>();
            services.AddScoped<ISlotService, SlotService>();
            services.AddScoped<IPricingService, PricingService>();
            services.AddScoped<IDemandTrackingService, DemandTrackingService>();
            services.AddScoped<IDistributedLockService, DistributedLockService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IRefundService, RefundService>();
            services.AddScoped<IWaitlistService, WaitlistService>();
            services.AddScoped<IRatingService, RatingService>();

            // AutoMapper
            services.AddAutoMapper(typeof(Mappings.MappingProfile));

            // Background services
            services.AddHostedService<GameAutoCancelService>();
            services.AddHostedService<SlotLockExpiryService>();
            services.AddHostedService<RefundProcessingService>();
            services.AddHostedService<DiscountExpiryService>();
            services.AddHostedService<WaitlistCleanupService>();

            return services;
        }
    }
}
