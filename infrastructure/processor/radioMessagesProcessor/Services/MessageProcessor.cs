using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using RadioMessagesProcessor.Helpers;
using RadioMessagesProcessor.Services;
using System;
using System.Text;
using AutoMapper;
using RadioMessagesProcessor.Entities;

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
        IMapper mapper;

        public MessageProcessor(IServiceProvider serviceProvider,
            IMapper mapper,
            IOptions<AppSettings> appSettingsProvider, 
            IRadioLocationMessagesService radioLocationMessagesService, 
            IDecoder decoder)
        {
            this.mapper = mapper;
            this.appSettings = appSettingsProvider.Value;
            this.radioLocationMessagesService = radioLocationMessagesService;
            this.decoder = decoder;
            this.logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<MessageProcessor>();
        }

        public void Run()
        {
            using (var consumer = new Consumer<Null, string>(this.appSettings.KafkaConsumer, null, new StringDeserializer(Encoding.UTF8)))
            {
                consumer.Subscribe(new string[] { this.appSettings.MessagesTopic });

                consumer.OnConsumeError += (_, msg) =>
                {
                    this.logger.LogError($"CONSUME ERROR: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} \n{msg.Value}");
                };

                consumer.OnError += (_, ex) =>
                {
                    this.logger.LogError($"ERROR: {ex}");
                };

                consumer.OnMessage += (_, msg) =>
                {
                    this.logger.LogTrace($"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} \n{msg.Value}");

                    // the message is processed
                    var rlm = this.decoder.FromRawEvent(msg.Value, out bool didParse, out string parse_error_message);

                    if (!didParse)
                    {
                        this.logger.LogWarning(parse_error_message);
                        // todo - add the errnous message to poison queue
                    }
                    else
                    {
                        var didDecode = this.decoder.Decode(rlm, out string decode_error_message);
                        if (!didDecode)
                        {
                            this.logger.LogWarning(decode_error_message);
                            // todo - add the errnous message to poison queue
                        }
                        else
                        {
                            this.radioLocationMessagesService.Insert(this.mapper.Map<RadioLocationMessage>(rlm));
                        }
                    }

                    // move on
                    consumer.CommitAsync(msg);
                };

                Console.WriteLine("Incomming mesage are to be displayed here:");
                while (true)
                {
                    consumer.Poll(10);
                }
            }

        }
    }
}
