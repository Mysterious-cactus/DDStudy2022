using AutoMapper;
using Common;

namespace Api
{
    //описание карты маппинга
    public class MapperProfile: Profile
    {
        public MapperProfile()
        {
            //указание, что делать с конкретными свойствами при маппинге
            CreateMap<Models.CreateUserModel, DAL.Entities.User>()
            .ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
            .ForMember(d => d.PasswordHash, m => m.MapFrom(s => HashHelper.GetHash(s.Password)))
            //в связи с особенностями UTC
            .ForMember(d => d.BirthDay, m => m.MapFrom(s => s.BirthDay.UtcDateTime));
            CreateMap<DAL.Entities.User, Models.UserModel>();
            CreateMap<DAL.Entities.Avatar, Models.AttachModel>();
            CreateMap<DAL.Entities.Post, Models.GetPostRequestModel>().ForMember(c => c.UserId, m => m.MapFrom(s => s.Author.Id));
            CreateMap<DAL.Entities.Comment, Models.GetCommentsRequestModel>().ForMember(c => c.UserId, m => m.MapFrom(s => s.Author.Id));
        }
    }
}
