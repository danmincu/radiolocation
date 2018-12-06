namespace LocationData.Entities
{
    public class CellInfo
    {
        public string Radio { get; set; }
        public string Mcc { get; set; }
        public string Mnc { get; set; }
        public string Lac { get; set; }
        public string Cid { get; set; }
        public string PscPci { get; set; }
        public int Rssi { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string IsReg { get; set; }
    }
}
