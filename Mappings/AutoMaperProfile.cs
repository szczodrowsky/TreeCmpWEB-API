using AutoMapper;
using TreeCmpWebAPI.Models.Domain;
using TreeCmpWebAPI.Models.DTO;

namespace TreeCmpWebAPI.Mappings
{
    public class AutoMaperProfile:Profile
    {
        public AutoMaperProfile()
        {
            CreateMap<Newick, NewickDto>().ReverseMap();
            CreateMap<AddNewickRequestDto, Newick>().ReverseMap();
            CreateMap<UpdateNewickRequestDto,Newick>().ReverseMap();
        }
    }
}
