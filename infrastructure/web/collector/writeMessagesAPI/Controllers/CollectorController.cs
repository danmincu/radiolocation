using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using WriteMessagesApi.Helpers;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using WriteMessagesApi.Dtos;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;

namespace WriteMessagesApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CollectorController : ControllerBase
    {
        private IMapper _mapper;
        private readonly AppSettings _appSettings;
        private static Producer<Null, string> KafkaProducer;
        private static string messagesTopic = null;

        public CollectorController(
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _mapper = mapper;
            _appSettings = appSettings.Value;
            
            // lazy load
            if (KafkaProducer == null)
            {
                var kafkaConfig = new Dictionary<string, object>
                {
                    { "bootstrap.servers", _appSettings.KafkaBoostrapServers }
                };
                KafkaProducer = new Producer<Null, string>(kafkaConfig, null, new StringSerializer(Encoding.UTF8));
            }
             messagesTopic = messagesTopic ?? _appSettings.MessagesTopic;

        }

        [AllowAnonymous]
        [HttpPost("radioLocation")]
        public IActionResult RadioLocation([FromBody]MessageDto message)
        {
            var locationText = message.Location;


            KafkaProducer.OnLog += (_, msg) =>
                       {
                           System.Diagnostics.Trace.WriteLine($"LOG: {msg.Message}");
                       };

            KafkaProducer.OnError += (_, ex) =>
                       {
                           System.Diagnostics.Trace.WriteLine($"ERROR: {ex}");
                       };


            KafkaProducer.ProduceAsync(messagesTopic, null, locationText);

            return Ok(new
            {
                //todo - here we can send instructions for the andoroid app
                Id = "Ok!"
            });
        }
    }
}
