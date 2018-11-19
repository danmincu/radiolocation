using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace RadioMessagesProcessor.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }

        public string MessagesTopic { get; set; }

        public LogLevel logLevel { get; set; }

        public string SolrCellSitesServer { get; set; }

        public string SolrCellSitesServerCore { get; set; }

        public Dictionary<string, object> KafkaConsumer { get; set; }
    }
}