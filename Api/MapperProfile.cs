﻿using AutoMapper;
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
            .ForMember(d => d.BirthDate, m => m.MapFrom(s => s.BirthDate.UtcDateTime));
            CreateMap<DAL.Entities.User, Models.UserModel>(); ;
        }
    }
}
