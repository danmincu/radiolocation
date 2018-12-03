using AutoMapper;
using Mapping.Radio;
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
                    m => m.ResolveUsing(s => JsonConvert.DeserializeObject<IEnumerable<CellInfoDto>>(ZipUnzip.Unzip(s.DecodedEvent)))
                    )
                .ForMember(
                    d => d.RadioShapes,
                    m => m.ResolveUsing(s => s.RadioShapes == null ? null :
                      JsonConvert.DeserializeObject<RadioIntersection.RadioIntersectionResponse>(ZipUnzip.Unzip(s.RadioShapes)))
                    )
                .ForMember(
                    d => d.RawEventString,
                    m => m.ResolveUsing(s => ZipUnzip.Unzip(s.RawEvent))
                    );

            CreateMap<RadioLocationMessageDto, RadioLocationMessage>()
                .ForMember(d => d.DecodedEvent, m => m.ResolveUsing(s => ZipUnzip.Zip(JsonConvert.SerializeObject(s.Cells))))
                .ForMember(d => d.RadioShapes, m => m.ResolveUsing(s => s.RadioShapes == null ? null :
                        ZipUnzip.Zip(JsonConvert.SerializeObject(s.RadioShapes))))
                .ForMember(d => d.RawEvent, m => m.ResolveUsing(s => string.IsNullOrEmpty(s.RawEventString) ? null :
                        ZipUnzip.Zip(s.RawEventString)));



            CreateMap<RadioCellInfoDto, CellInfoDto>();
            CreateMap<CellInfoDto, RadioCellInfoDto>();

            CreateMap<CellInfo, CellInfoDto>();
            CreateMap<CellInfoDto, CellInfo>();
        }
    }
}