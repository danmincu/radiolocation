using Mapping.ClipperLib;
using System.Collections.Generic;

namespace Mapping.Radio
{
    public class RadioInfoGps
    {
        public RadioInfoGps(string radio, int rssi, double longitude, double latitude)
        {
            this.Radio = radio;
            this.Rssi = rssi;
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        public string Radio { get; set; }
        public int Rssi { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public override string ToString()
        {
            return string.Format($"long:{this.Longitude} lat:{this.Latitude}");
        }
    }

}
