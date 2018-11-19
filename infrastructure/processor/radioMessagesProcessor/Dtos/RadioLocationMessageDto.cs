using System;
using System.Collections.Generic;

namespace RadioMessagesProcessor.Dtos
{
    public class RadioLocationMessageDto
    {
        public RadioLocationMessageDto() : this(Guid.NewGuid()) { }

        public RadioLocationMessageDto(Guid id)
        {
            this.Cells = new List<CellInfoDto>();
            this.Id = id;
        }

        public void AddCell(CellInfoDto cidto)
        {
            ((IList<CellInfoDto>)this.Cells).Add(cidto);
        }

        public System.Guid Id { get; set; }

        public string Imei { get; set; }

        public System.DateTime CollectionDateUTC { get; set; }

        public System.DateTime DeviceDate { get; set; }

        public byte[] RawEvent { get; set; }

        public System.DateTime DecodedDateUTC { get; set; }

        public double GpsLatitude { get; set; }

        public double GpsLongitude { get; set; }

        public long GpsAge { get; set; }

        public double GpsAccuracy { get; set; }

        public double GpsSpeed { get; set; }

        public double? GpsBearing { get; set; }

        public double DecodedLatitude { get; set; }

        public double DecodedLongitude { get; set; }

        public IEnumerable<CellInfoDto> Cells { get; set; }
    }

}