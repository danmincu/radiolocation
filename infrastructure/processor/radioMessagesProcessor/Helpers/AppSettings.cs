using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace RadioMessagesProcessor.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }

        public string MessagesTopic { get; set; }

        public LogLevel logLevel { get; set; }

        public Dictionary<string, object> KafkaConsumer { get; set; }
    }

    //public class KafkaConsumer
    //{
    //    public string 
    //}
}