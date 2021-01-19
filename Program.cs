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

            var card = new Notecard();            
            Console.WriteLine(card.Transaction("{ \"req\": \"card.status\" }"));
        } 
    }
}
