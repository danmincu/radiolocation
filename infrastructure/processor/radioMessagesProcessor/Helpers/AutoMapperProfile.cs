using AutoMapper;
using RadioMessagesProcessor.Dtos;
using RadioMessagesProcessor.Entities;

namespace RadioMessagesProcessor.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RadioLocationMessage, RadioLocationMessageDto>();
            CreateMap<RadioLocationMessageDto, RadioLocationMessage>();

            CreateMap<RadioCellInfoDto, CellInfoDto>();
            CreateMap<CellInfoDto, RadioCellInfoDto>();

            CreateMap<CellInfo, CellInfoDto>();
            CreateMap<CellInfoDto, CellInfo>();
        }
    }
}