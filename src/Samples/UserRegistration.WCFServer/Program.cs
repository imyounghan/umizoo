

namespace UserRegistration.Application
{
    using System;

    using Umizoo.Configurations;

    class Program
    {
        static void Main(string[] args)
        {
            Configuration.Current
                .UseKafka(ProcessingFlags.Command | ProcessingFlags.Query)
                .EnableService(ConnectionMode.Wcf)
                .Done();

            Console.WriteLine("type 'ESC' key to exit service...");
            while (Console.ReadKey().Key == ConsoleKey.Escape) {
                break;
            }

            //Console.ForegroundColor = ConsoleColor.Gray;
            Console.ReadKey();
        }
    }
}
