using AutoMapper;
using Internet_1.Models;
using Internet_1.ViewModels;

namespace Internet_1.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            CreateMap<Category, CategoryModel>().ReverseMap();
            CreateMap<AppUser, UserModel>().ReverseMap();
            CreateMap<AppUser, RegisterModel>().ReverseMap();

            // Meeting mapping'ini ekleyin
            CreateMap<Meeting, MeetingModel>()
                .ForMember(dest => dest.MeetingDate,
                          opt => opt.MapFrom(src => src.MeetingDate.ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.MeetingTime,
                          opt => opt.MapFrom(src => src.MeetingTime.ToString(@"hh\:mm")));

            // Ters yöndeki mapping için (MeetingModel'den Meeting'e)
            CreateMap<MeetingModel, Meeting>()
                .ForMember(dest => dest.MeetingDate,
                          opt => opt.MapFrom(src => DateTime.Parse(src.MeetingDate)))
                .ForMember(dest => dest.MeetingTime,
                          opt => opt.MapFrom(src => TimeSpan.Parse(src.MeetingTime)));
        }
    }
}
