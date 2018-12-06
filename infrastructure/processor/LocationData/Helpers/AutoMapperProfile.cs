using AutoMapper;
using Mapping.Radio;
using Newtonsoft.Json;
using LocationData.Dtos;
using LocationData.Entities;
using System.Collections.Generic;

namespace LocationData.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RadioLocationMessage, RadioLocationMessageDto>()
                .ForMember(
                    d => d.Cells,
                    m => m.MapFrom(s => JsonConvert.DeserializeObject<IEnumerable<CellInfoDto>>(ZipUnzip.Unzip(s.DecodedEvent)))
                    )
                .ForMember(
                    d => d.RadioShapes,
                    m => m.MapFrom(s => s.RadioShapes == null ? null :
                      JsonConvert.DeserializeObject<RadioIntersection.RadioIntersectionResponse>(ZipUnzip.Unzip(s.RadioShapes)))
                    )
                .ForMember(
                    d => d.RawEventString,
                    m => m.MapFrom(s => ZipUnzip.Unzip(s.RawEvent))
                    );

            CreateMap<RadioLocationMessageDto, RadioLocationMessage>()
                .ForMember(d => d.DecodedEvent, m => m.MapFrom(s => ZipUnzip.Zip(JsonConvert.SerializeObject(s.Cells))))
                .ForMember(d => d.RadioShapes, m => m.MapFrom(s => s.RadioShapes == null ? null :
                        ZipUnzip.Zip(JsonConvert.SerializeObject(s.RadioShapes))))
                .ForMember(d => d.RawEvent, m => m.MapFrom(s => string.IsNullOrEmpty(s.RawEventString) ? null :
                        ZipUnzip.Zip(s.RawEventString)));



            CreateMap<RadioCellInfoDto, CellInfoDto>();
            CreateMap<CellInfoDto, RadioCellInfoDto>();

            CreateMap<CellInfo, CellInfoDto>();
            CreateMap<CellInfoDto, CellInfo>();
        }
    }
}