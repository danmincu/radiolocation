using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using RadioMessagesProcessor.Helpers;
using RadioMessagesProcessor.Services;
using System;
using System.Text;

namespace radioMessagesProcessor.Services
{
    public interface IMessageProcessor
    {
        void Run();
    }

    public class MessageProcessor : IMessageProcessor
    {
        AppSettings appSettings;
        IRadioLocationMessagesService radioLocationMessagesService;
        IDecoder decoder;
        ILogger<MessageProcessor> logger;

        public MessageProcessor(IServiceProvider serviceProvider,
            IOptions<AppSettings> appSettingsProvider, 
            IRadioLocationMessagesService radioLocationMessagesService, 
            IDecoder decoder)
        {
            this.appSettings = appSettingsProvider.Value;
            this.radioLocationMessagesService = radioLocationMessagesService;
            this.decoder = decoder;
            this.logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<MessageProcessor>();
        }

        public void Run()
        {
            //var config = new Dictionary<string, object>
            //          {
            //              { "group.id", "sample-consumer" },
            //              { "bootstrap.servers", "192.168.1.8:9092" },
            //              { "enable.auto.commit", "false"},
            //              {"auto.offset.reset", "latest" }
            //          };

            using (var consumer = new Consumer<Null, string>(this.appSettings.KafkaConsumer, null, new StringDeserializer(Encoding.UTF8)))
            {
                consumer.Subscribe(new string[] { this.appSettings.MessagesTopic });

                consumer.OnConsumeError += (_, msg) =>
                {
                    this.logger.LogError($"CONSUME ERROR: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} \n{msg.Value}");
                    //Console.WriteLine($"CONSUME ERROR: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");
                };

                consumer.OnError += (_, ex) =>
                {
                    //Console.WriteLine($"ERROR: {ex}");
                    this.logger.LogError($"ERROR: {ex}");
                };

                consumer.OnMessage += (_, msg) =>
                {
                    this.logger.LogTrace($"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} \n{msg.Value}");
                    //Console.WriteLine($"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");
                    consumer.CommitAsync(msg);
                };

                Console.WriteLine("Incomming mesage are to be displayed here:");
                while (true)
                {
                    consumer.Poll(30);
                }
            }

        }
    }
}
