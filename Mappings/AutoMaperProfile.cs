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
            CreateMap<TreeCmpRequestDto, TreeCmp>()
           .ForMember(dest => dest.InputFile, opt => opt.Ignore())
           .ForMember(dest => dest.RefTreeFile, opt => opt.Ignore())
           .ForMember(dest => dest.OutputFile, opt => opt.Ignore())
           .ForMember(dest => dest.UserName, opt => opt.Ignore())
           .ForMember(dest => dest.Metrics, opt => opt.Ignore());
        }
    }
}
