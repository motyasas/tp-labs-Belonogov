using System.Net.Sockets;
using System.Text;
using ChatServer;

// Запуск:  dotnet run --project ChatServer            (порт 5555)
//          dotnet run --project ChatServer -- 6000    (порт 6000)

Console.OutputEncoding = Encoding.UTF8;

const int DefaultPort = 5555;
int port = DefaultPort;

// args - аргументы командной строки. Если первый аргумент есть - это порт.
if (args.Length > 0)
{
    if (!int.TryParse(args[0], out port) || port < 1 || port > 65535)
    {
        Console.WriteLine("Ошибка: порт должен быть числом от 1 до 65535.");
        return;
    }
}

Server server = new Server(port);
Console.WriteLine("Ctrl+C - остановка сервера.");

try
{
    await server.RunAsync();
}
catch (SocketException ex)
{
    // Например, порт уже занят другой программой
    Console.WriteLine($"Не удалось запустить сервер на порту {port}: {ex.Message}");
}
