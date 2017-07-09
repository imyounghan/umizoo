

namespace UserRegistration.Processor.Event
{
    using System;

    using Umizoo.Configurations;

    class Program
    {
        static void Main(string[] args)
        {
            Configuration.Current
                .UseKafka(ProcessingFlags.Command | ProcessingFlags.Event | ProcessingFlags.Query, "Events", "Queries")
                .EnableProcessors(ProcessingFlags.Event | ProcessingFlags.Query, ConnectionMode.Wcf)
                .Done();

            Console.WriteLine("type 'ESC' key to exit event consumer...");
            while(Console.ReadKey().Key == ConsoleKey.Escape) {
                break;
            }

            Console.ReadKey();
        }
    }
}
