﻿using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using RadioMessagesProcessor.Helpers;
using RadioMessagesProcessor.Services;
using System;
using System.Text;
using AutoMapper;
using LocationData.Entities;
using LocationData.Helpers;
using Newtonsoft.Json;
using LocationData;

namespace radioMessagesProcessor.Services
{
    public interface IMessageProcessor
    {
        void Run();
    }

    public class MessageProcessor : IMessageProcessor
    {
        AppSettings appSettings;
        IServiceProvider serviceProvider;
        IDecoder decoder;
        ILogger<MessageProcessor> logger;
        IMapper mapper;

        public MessageProcessor(IServiceProvider serviceProvider,
            IMapper mapper,
            IOptions<AppSettings> appSettingsProvider,
            IDecoder decoder)
        {   
            this.mapper = mapper;
            this.appSettings = appSettingsProvider.Value;
            this.serviceProvider = serviceProvider;
            this.decoder = decoder;
            this.logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<MessageProcessor>();

            //using (var serviceScope = serviceProvider.CreateScope())
            //{
            //    var message = new RadioLocationMessagesService(
            //        serviceScope.ServiceProvider.GetService<DataContext>())
            //        .GetById(id: Guid.Parse("b85e2989-411c-41eb-ae7a-2a87d1ef1d5a"));
            //    var mdto = mapper.Map<RadioLocationMessageDto>(message);
            //    var m = mdto.RawEventString;
            //    var didDecode = this.decoder.DecodeAsync(mdto).Result;
            //    var radioIntersection = Mapping.Radio.RadioIntersection.GenerateRadioIntersection(
            //        mdto
            //        .Cells
            //        .Where(c => c.IsDecoded)
            //        .Select(c => new Mapping.Radio.RadioInfoGps(c.Radio, c.Rssi, c.Longitude, c.Latitude)).ToList());
            //    var ri = radioIntersection;
            //}

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

                consumer.OnMessage += async (_, msg) =>
                {
                    this.logger.LogTrace($"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} \n{msg.Value}");

                    try
                    {
                        // the message is processed
                        var rlm = this.decoder.FromRawEvent(msg.Value, out bool didParse, out string parse_error_message);

                        if (!didParse)
                        {
                            this.logger.LogWarning(parse_error_message);
                            // todo - add the errnous message to poison queue
                        }
                        else
                        // filter if you want if (rlm.DeviceDate.ToLocalTime().Day == 18 && rlm.DeviceDate.ToLocalTime().Hour >= 13 && rlm.DeviceDate.ToLocalTime().Hour <= 18)
                        {
                            DecodeResult didDecode = new DecodeResult() { Success = false };
                            try
                            {
                                didDecode = await this.decoder.DecodeAsync(rlm).ConfigureAwait(false);
                            }
                            catch(Exception ex)
                            {
                                //burry
                                 logger.LogError($"Error decoding {rlm.ToFriendlyName()} Exception: {ex}");
                            }
                            if (!didDecode.Success)
                            {
                                this.logger.LogWarning(didDecode.ErrorMessage);
                                // todo - add the errnous message to poison queue
                            }
                            else
                            {

                                // this is just debug info - I should read this from database
                                try
                                {
                                    await GoogleEarthPlacesCreator.ToPlaceFile(rlm);
                                }
                                catch
                                {
                                    //burry don't care much if a file won't get created
                                }
                                // end debug info

                                // create on demand this service to insure I get a transient db context

                                using (var serviceScope = serviceProvider.CreateScope())
                                {
                                    var m = rlm.RadioShapes == null ? null : ZipUnzip.Zip(JsonConvert.SerializeObject(rlm.RadioShapes));

                                    var v = this.mapper.Map<RadioLocationMessage>(rlm);

                                    await new RadioLocationMessagesService(
                                        serviceScope.ServiceProvider.GetService<LocationDataContext>())
                                        .InsertAsync(this.mapper.Map<RadioLocationMessage>(rlm))
                                        .ConfigureAwait(false);
                                    //this.serviceProvider.GetService<IRadioLocationMessagesService>().InsertAsync(this.mapper.Map<RadioLocationMessage>(rlm)).ConfigureAwait(false);
                                }
                            }
                        }

                        // move on
                        await consumer.CommitAsync(msg).ConfigureAwait(false);
                    }
                    // these are the exceptions that are blocking the queue
                    catch (Exception ex)
                    {
                        this.logger.LogCritical($"Deque from Kafka stopped due to:{ex.Message}");
                    }
                };

                Console.WriteLine("Incomming mesage are to be displayed here:");
                while (true)
                {
                    System.Threading.Thread.Sleep(30);
                    consumer.Poll(100);
                }
            }

        }
    }
}
