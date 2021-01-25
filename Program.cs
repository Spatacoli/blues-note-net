using System;
using System.Device.I2c;
using System.IO;
using System.Text;
using System.Text.Json;

namespace blues_note_net
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            try
            {
                var card = new Notecard();            
                Console.WriteLine(card.Transaction("{ \"req\": \"card.status\" }"));

                var data = new DataPoint() { temp = 35.5, humidity = 56.23 };
                var req = new Request() { req = "note.add", body = data };
                Console.WriteLine(card.Transaction(JsonSerializer.Serialize(req)));
                Console.WriteLine(card.Transaction("{ \"req\": \"hub.sync\" }"));
            } 
            catch (TimeoutException te) 
            {
                Console.WriteLine(te.Message);
            }
        } 
    }

    class Request 
    {
        public string req { get; set; }
        public DataPoint body { get; set; }
    }

    class DataPoint
    {
        public double temp { get; set; }
        public double humidity { get; set; }
    }
}
