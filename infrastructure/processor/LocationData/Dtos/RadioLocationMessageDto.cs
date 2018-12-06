using System;
using System.Linq;
using System.Collections.Generic;
using Mapping.Radio;

namespace LocationData.Dtos
{
    public class RadioLocationMessageDto
    {
        private List<CellInfoDto> cells;

        public RadioLocationMessageDto() : this(Guid.NewGuid()) { }

        public RadioLocationMessageDto(Guid id)
        {
            this.cells = new List<CellInfoDto>();
            this.Id = id;
        }

        public IEnumerable<CellInfoDto> CellsExcept(CellInfoDto except)
        {
            return this.Cells.Where(c => !(c.Mcc.Equals(except.Mcc, StringComparison.OrdinalIgnoreCase) &&
                    c.Mnc.Equals(except.Mnc, StringComparison.OrdinalIgnoreCase) &&
                    c.Cid.Equals(except.Cid, StringComparison.OrdinalIgnoreCase) &&
                    c.Lac.Equals(except.Lac, StringComparison.OrdinalIgnoreCase)));
        }

        public void AddCell(CellInfoDto cidto)
        {
            ((IList<CellInfoDto>)this.Cells).Add(cidto);
        }

        public System.Guid Id { get; set; }

        public string Imei { get; set; }

        public System.DateTime CollectionDateUTC { get; set; }

        public System.DateTime DeviceDate { get; set; }

        public string RawEventString { get; set; }

        public System.DateTime DecodedDateUTC { get; set; }

        public RadioIntersection.RadioIntersectionResponse RadioShapes
        {
            get; set;
        }

        public double GpsLatitude { get; set; }

        public double GpsLongitude { get; set; }

        public long GpsAge { get; set; }

        public double GpsAccuracy { get; set; }

        public double GpsSpeed { get; set; }

        public double? GpsBearing { get; set; }

        public int Rssi { get; set; }

        public double DecodedLatitude { get; set; }

        public double DecodedLongitude { get; set; }

        public IEnumerable<CellInfoDto> Cells
        {
            get
            {
                return this.cells;
            }
            set
            {
                foreach (var item in value ?? Enumerable.Empty<CellInfoDto>())
                {
                    this.AddCell(item);
                }
            }
        }

        public string ToFriendlyName()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var cell in this.Cells)
            {
                sb.AppendLine(cell.ToFriendlyName());
            }
            return sb.ToString();
        }
    }

}