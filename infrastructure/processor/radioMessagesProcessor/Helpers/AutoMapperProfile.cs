using AutoMapper;
using Newtonsoft.Json;
using radioMessagesProcessor.Helpers;
using RadioMessagesProcessor.Dtos;
using RadioMessagesProcessor.Entities;
using System.Collections.Generic;

namespace RadioMessagesProcessor.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RadioLocationMessage, RadioLocationMessageDto>()
                .ForMember(
                    d => d.Cells, 
                    m => m.ResolveUsing(s => JsonConvert.DeserializeObject<IEnumerable<CellInfoDto>>(ZipUnzip.Unzip(s.DecodedEvent))));

            CreateMap<RadioLocationMessageDto, RadioLocationMessage>()
                .ForMember(d => d.DecodedEvent, m => m.ResolveUsing(s => ZipUnzip.Zip(JsonConvert.SerializeObject(s.Cells))));

            CreateMap<RadioCellInfoDto, CellInfoDto>();
            CreateMap<CellInfoDto, RadioCellInfoDto>();

            CreateMap<CellInfo, CellInfoDto>();
            CreateMap<CellInfoDto, CellInfo>();
        }
    }
}