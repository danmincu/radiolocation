namespace RadioMessagesProcessor.Entities
{
    public class RadioLocationMessage
    {
        public System.Guid Id { get; set; }

        public string Imei { get; set; }

        public System.DateTime CollectionDateUTC { get; set; }

        public System.DateTime DeviceDate { get; set; }

        public byte[] RawEvent { get; set; }

        public byte[] DecodedEvent { get; set; }

        public System.DateTime DecodedDateUTC { get; set; }

        public double GpsLatitude { get; set; }

        public double GpsLongitude { get; set; }

        public double DecodedLatitude { get; set; }

        public double DecodedLongitude { get; set; }
    }

}