using AutoMapper;
using DataNex.Model.Dtos;
using DataNex.Model.Models;

namespace DataNexApi.Mapping
{
    public class AutoMapperProfiles:Profile
    {

        public AutoMapperProfiles()
        {
            CreateMap<User,UserDto>().ReverseMap();
            CreateMap<Customer,CustomerDto>().ReverseMap();

            CreateMap<Document, DocumentDto>().ReverseMap();
        }
    }
}
