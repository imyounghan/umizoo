

namespace UserRegistration.Processor
{
    using System;
    using Umizoo.Configurations;

    class Program
    {
        static void Main(string[] args)
        {
            Configuration.Create()
                .UseKafka(ProcessingFlags.Event | ProcessingFlags.Command, "Commands")
                .EnableProcessors(ProcessingFlags.Command)
                .EnableService(ConnectionMode.Wcf)
                .Done();

            Console.WriteLine("type 'ESC' key to exit command consumer...");
            while(Console.ReadKey().Key == ConsoleKey.Escape) {
                break;
            }

            Console.ReadKey();
        }
    }
}
