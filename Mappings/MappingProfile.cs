using AutoMapper;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User Mappings
            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            CreateMap<User, UserDto>();

            // Venue Mappings
            CreateMap<CreateVenueDto, Venue>();
            CreateMap<UpdateVenueDto, Venue>();
            CreateMap<Venue, VenueDto>()
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.UserName));

            // Court Mappings
            CreateMap<CreateCourtDto, Court>();
            CreateMap<UpdateCourtDto, Court>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Court, CourtDto>()
                .ForMember(dest => dest.VenueName, opt => opt.MapFrom(src => src.Venue.Name));

            // Discount Mappings
            CreateMap<CreateDiscountDto, Discount>();
            CreateMap<UpdateDiscountDto, Discount>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Discount, DiscountDto>();

            // Slot Mappings
            CreateMap<Slot, SlotDto>()
                .ForMember(dest => dest.CourtName, opt => opt.MapFrom(src => src.Court.Name));
            CreateMap<Slot, AvailableSlotDto>()
                .ForMember(dest => dest.CourtName, opt => opt.MapFrom(src => src.Court.Name))
                .ForMember(dest => dest.VenueName, opt => opt.MapFrom(src => src.Court.Venue.Name));
            CreateMap<Slot, BookSlotResponseDto>()
                .ForMember(dest => dest.SlotId, opt => opt.MapFrom(src => src.Id));

            // Game Mappings
            CreateMap<CreateGameDto, Game>();
            CreateMap<Game, GameDto>()
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser.UserName))
                .ForMember(dest => dest.CurrentPlayerCount, opt => opt.MapFrom(src => src.Players.Count));

            CreateMap<GamePlayer, GamePlayerDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

            // Wallet Mappings
            CreateMap<Wallet, WalletDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

            CreateMap<Transaction, TransactionDto>();
            CreateMap<Transaction, PaymentResponseDto>()
                .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SlotId, opt => opt.MapFrom(src => src.RelatedSlotId));

            CreateMap<Refund, RefundDto>();

            // Waitlist Mappings
            CreateMap<Waitlist, WaitlistDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

            // Rating Mappings
            CreateMap<CreateRatingDto, Rating>();
            CreateMap<Rating, RatingDto>()
                .ForMember(dest => dest.RatedByName, opt => opt.MapFrom(src => src.RatedBy.UserName))
                .ForMember(dest => dest.VenueName, opt => opt.MapFrom(src => src.Venue.Name))
                .ForMember(dest => dest.CourtName, opt => opt.MapFrom(src => src.Court.Name))
                .ForMember(dest => dest.PlayerName, opt => opt.MapFrom(src => src.Player.UserName));
        }
    }
}
