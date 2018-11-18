namespace WriteMessagesApi.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string KafkaBoostrapServers { get; set; }
        public string MessagesTopic { get; set; }
    }
}