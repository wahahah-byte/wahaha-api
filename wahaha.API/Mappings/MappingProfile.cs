using AutoMapper;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTO;

namespace wahaha.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // AvatarItem
        CreateMap<AvatarItem, AvatarItemDto>()
            .ForMember(dest => dest.Slot, opt => opt.MapFrom(src => src.Slot.ToString()))
            .ForMember(dest => dest.Rarity, opt => opt.MapFrom(src => src.Rarity.ToString()));

        CreateMap<CreateAvatarItemDto, AvatarItem>()
            .ForMember(dest => dest.Slot, opt => opt.MapFrom(src => Enum.Parse<ItemSlot>(src.Slot, true)))
            .ForMember(dest => dest.Rarity, opt => opt.MapFrom(src => Enum.Parse<Rarity>(src.Rarity, true)))
            .ForMember(dest => dest.PreviewAssetUrl, opt => opt.Ignore())  // handled by BlobService
            .ForMember(dest => dest.UserInventories, opt => opt.Ignore());

        CreateMap<UpdateAvatarItemDto, AvatarItem>()
            .ForMember(dest => dest.Slot, opt => opt.MapFrom(src => Enum.Parse<ItemSlot>(src.Slot, true)))
            .ForMember(dest => dest.Rarity, opt => opt.MapFrom(src => Enum.Parse<Rarity>(src.Rarity, true)))
            .ForMember(dest => dest.PreviewAssetUrl, opt => opt.Ignore())  // handled by BlobService
            .ForMember(dest => dest.UserInventories, opt => opt.Ignore());

        // Minigame
        CreateMap<Minigame, MinigameDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Difficulty, opt => opt.MapFrom(src => src.Difficulty.ToString()));

        CreateMap<CreateMinigameDto, Minigame>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<GameType>(src.Type, true)))
            .ForMember(dest => dest.Difficulty, opt => opt.MapFrom(src => Enum.Parse<Difficulty>(src.Difficulty, true)));

        CreateMap<UpdateMinigameDto, Minigame>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<GameType>(src.Type, true)))
            .ForMember(dest => dest.Difficulty, opt => opt.MapFrom(src => Enum.Parse<Difficulty>(src.Difficulty, true)));

        // MinigameSession
        CreateMap<MinigameSession, MinigameSessionDto>()
            .ForMember(dest => dest.Outcome, opt => opt.MapFrom(src => src.Outcome.ToString()))
            .ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Minigame != null ? src.Minigame.Name : null))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Username : null));

        CreateMap<MinigameSession, MinigameSessionLeaderboardDto>()
            .ForMember(dest => dest.Outcome, opt => opt.MapFrom(src => src.Outcome.ToString()))
            .ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Minigame != null ? src.Minigame.Name : null))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty));

        CreateMap<CreateMinigameSessionDto, MinigameSession>()
            .ForMember(dest => dest.Outcome, opt => opt.MapFrom(src => Enum.Parse<SessionOutcome>(src.Outcome, true)))
            .ForMember(dest => dest.PlayedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // PointTransaction
        CreateMap<PointTransaction, PointTransactionDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.SourceType, opt => opt.MapFrom(src => src.SourceType != null ? src.SourceType.ToString() : null));

        CreateMap<CreatePointTransactionDto, PointTransaction>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<TransactionType>(src.Type, true)))
            .ForMember(dest => dest.SourceType, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.SourceType) ? Enum.Parse<SourceType>(src.SourceType, true) : (SourceType?)null))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Streak
        CreateMap<Streak, StreakDto>();

        CreateMap<CreateStreakDto, Streak>()
            .ForMember(dest => dest.LastActivityDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Task
        CreateMap<Models.Domain.Task, TaskDto>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreateTaskDto, Models.Domain.Task>()
            .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => Enum.Parse<Priority>(src.Priority, true)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<ByteTaskStatus>(src.Status, true)))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateTaskDto, Models.Domain.Task>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => Enum.Parse<Priority>(src.Priority, true)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<ByteTaskStatus>(src.Status, true)));

        // UserInventory
        CreateMap<UserInventory, UserInventoryDto>()
            .ForMember(dest => dest.AvatarItem, opt => opt.MapFrom(src => src.AvatarItem));

        CreateMap<CreateUserInventoryDto, UserInventory>()
            .ForMember(dest => dest.AcquiredAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Users
        CreateMap<Users, UserDto>()
            .ForMember(dest => dest.PointsSubmittedToday, opt => opt.MapFrom(src =>
                src.PointTransactions
                    .Where(pt => pt.Type == TransactionType.EARN && pt.CreatedAt.Date == DateTime.UtcNow.Date)
                    .Sum(pt => pt.Amount)));
        CreateMap<CreateUserDto, Users>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        CreateMap<UpdateUserDto, Users>();
    }
}
