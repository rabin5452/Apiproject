using AutoMapper;
using Practiseproject.DTO;
using Practiseproject.Model;

namespace Practiseproject.MapperProfile
{
    public class MapperProfile:Profile
    {
        public MapperProfile() 
        {
            CreateMap<Student,StudentDTO>().ReverseMap();
            CreateMap<User,AddUser>().ReverseMap();
            CreateMap<User,UserDTO>().ReverseMap(); 
        }
    }
}
