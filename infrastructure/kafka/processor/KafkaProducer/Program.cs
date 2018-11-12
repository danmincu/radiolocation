namespace KafkaProducer                                                                                                                    
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
        { "bootstrap.servers", "192.168.1.8:9092" }
      };

      using (var producer = new Producer<Null, string>(config, null, new StringSerializer(Encoding.UTF8)))
      {
        string text = null;

        Console.WriteLine("Start sending messages:");
        
        producer.OnLog +=  (_, msg) => 
        {
          Console.WriteLine($"LOG: {msg.Message}");         
        };

        producer.OnError +=  (_, ex) => 
        {
          Console.WriteLine($"ERROR: {ex}");        
        };



        while (text != "exit")
        {          
          text = Console.ReadLine();                                                                                                                                                              
          producer.ProduceAsync("incomming_radio_messages", null, text);
          Console.WriteLine($"Sending messages:{text}");
        }

        producer.Flush(100);
      }
    }
  }
}