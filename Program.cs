using System;
using System.Device.I2c;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

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
                
                var index = 0;

                while (index < 20)
                {
                    var response = JsonSerializer.Deserialize<TempResponse>(card.Transaction("{ \"req\": \"card.temp\" }"));
                
                    var req = new Request() { req = "note.add", body = response };
                    Console.WriteLine(card.Transaction(JsonSerializer.Serialize(req)));
                    Console.WriteLine(card.Transaction("{ \"req\": \"hub.sync\" }"));
                    Console.WriteLine(response);
                    Thread.Sleep(30000);
                    index++;
                }
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
        public TempResponse body { get; set; }
    }

    class TempResponse
    {
        public double value { get; set; } 
        public double calibration { get; set; }

        public override string ToString()
        {
            return $"Temperature is {value} and Calibration is {calibration}";
        }
    }
}
