using KSIS_server.UserData;

namespace KSIS_server
{
    internal class Program
    {
        static readonly Server server = new();

        static async Task Main()
        {
            DatabaseHelper.InitializeDatabase();

            var serverTask = Task.Run(() => server.StartServer());

            Console.WriteLine("Для остановки сервера введите: STOP");
            string? stop;
            while (true)
            {
                stop = Console.ReadLine();
                if (stop == "STOP")
                {
                    server.StopServer();
                    Console.WriteLine("Сервер отключён");
                    break;
                }
            }

            await serverTask;
        }
    }
}
