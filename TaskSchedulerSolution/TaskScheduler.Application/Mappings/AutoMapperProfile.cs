using AutoMapper;
using TaskScheduler.Application.DTOs.Auth;
using TaskScheduler.Application.DTOs.Tasks;
using TaskScheduler.Application.DTOs.Users;
using TaskScheduler.Domain.Entities;

namespace TaskScheduler.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User Mappings
            CreateMap<User, UserDto>();
            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.TotalTasks, opt => opt.MapFrom(src => src.Tasks.Count))
                .ForMember(dest => dest.CompletedTasks, opt => opt.MapFrom(src => src.Tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Completed)))
                .ForMember(dest => dest.PendingTasks, opt => opt.MapFrom(src => src.Tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Pending)));

            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Tasks, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.NotificationSettings, opt => opt.Ignore());

            // Task Mappings
            CreateMap<Domain.Entities.Task, TaskDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

            CreateMap<CreateTaskDto, Domain.Entities.Task>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.TaskStatus.Pending))
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Reminders, opt => opt.Ignore());

            CreateMap<UpdateTaskDto, Domain.Entities.Task>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Category Mappings
            CreateMap<Category, CategoryDto>();

            // Reminder Mappings
            CreateMap<Reminder, ReminderDto>();
        }

        // Helper DTOs for mapping
        public class CategoryDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Color { get; set; } = string.Empty;
        }

        public class ReminderDto
        {
            public int Id { get; set; }
            public DateTime ReminderTime { get; set; }
            public bool IsSent { get; set; }
        }
    }
}
