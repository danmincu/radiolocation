namespace KafkaConsumer
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using Confluent.Kafka;
  using Confluent.Kafka.Serialization;

  public class Program
  {
    static void Main(string[] args)
    {
      var config = new Dictionary<string, object>
      {
          { "group.id", "sample-consumer" },
          { "bootstrap.servers", "192.168.1.8:9092" },
          { "enable.auto.commit", "false"},
           {"auto.offset.reset", "latest" }
      };

      using (var consumer = new Consumer<Null, string>(config, null, new StringDeserializer(Encoding.UTF8)))
      {                
        consumer.Subscribe(new string[]{"incomming_radio_messages"});

        consumer.OnConsumeError +=  (_, msg) => 
        {
          Console.WriteLine($"CONSUME ERROR: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");         
        };

        consumer.OnError +=  (_, ex) => 
        {
          Console.WriteLine($"ERROR: {ex}");        
        };

        consumer.OnMessage += (_, msg) => 
        {
          Console.WriteLine($"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");
          consumer.CommitAsync(msg);
        };

        Console.WriteLine("Incomming mesage are to be displayed here:");
        while (true)
        {
            consumer.Poll(100);            
        }
      }
    }
  }
}