using API.DTO;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<AppUser, MemberDto>()//AppUser To MemberDto and
                .ForMember(m => m.Age, o => o.MapFrom(s => s.DateOfBirth.CalculateAge()))
                .ForMember(m => m.PhotoUrl, o => o.MapFrom(s => s.photos.FirstOrDefault(x => x.IsMain)!.Url));// This  is use for photo url
            CreateMap<Photo, PhotoDto>();//Photo to PhotoDto
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();//Data mapping RegisterDto to AppUser
            CreateMap<string, DateOnly>().ConvertUsing(s => DateOnly.Parse(s)); // s means string and DateOnly.Parse() is converting string to dateonly type.
            
            //ForMessage dto 
            CreateMap<Message, MessageDto>()
                .ForMember(m => m.SenderPhotoUrl, o => o.MapFrom(s => s.Sender.photos.FirstOrDefault(x => x.IsMain)!.Url)) // this is to mapping the senderphotourl and recipientphotourl from appuser main photo
                .ForMember(m => m.RecipientPhotoUrl, o => o.MapFrom(s => s.Recipient.photos.FirstOrDefault(x => x.IsMain)!.Url));
        }
    }
}
