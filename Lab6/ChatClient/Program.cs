using System.Net.Sockets;
using System.Text;

// Клиент чата. Запуск: dotnet run --project ChatClient

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8; // чтобы русские буквы правильно вводились

// ---------- Адрес и порт ----------

Console.Write("Адрес сервера (Enter = localhost): ");
string? host = Console.ReadLine();
if (string.IsNullOrWhiteSpace(host))
{
    host = "localhost";
}

Console.Write("Порт (Enter = 5555): ");
string? portInput = Console.ReadLine();
int port = 5555;
if (!string.IsNullOrWhiteSpace(portInput))
{
    if (!int.TryParse(portInput, out port) || port < 1 || port > 65535)
    {
        Console.WriteLine("Ошибка: порт должен быть числом от 1 до 65535.");
        return;
    }
}

// ---------- Подключение ----------

TcpClient client = new TcpClient();
try
{
    await client.ConnectAsync(host, port);
}
catch (SocketException)
{
    // Сервер не запущен, неверный адрес или порт
    Console.WriteLine($"Сервер {host}:{port} недоступен. Проверьте, что он запущен.");
    return;
}

NetworkStream stream = client.GetStream();
StreamReader reader = new StreamReader(stream);
StreamWriter writer = new StreamWriter(stream);
writer.AutoFlush = true; // каждая строка уходит на сервер сразу

try
{
    // ---------- Выбор ника ----------
    // Повторяем, пока сервер не ответит "OK"
    bool nickAccepted = false;
    while (!nickAccepted)
    {
        Console.Write("Ваш ник: ");
        string? nick = Console.ReadLine();
        if (nick == null)
        {
            return;
        }

        await writer.WriteLineAsync(nick);
        string? answer = await reader.ReadLineAsync();

        if (answer == null)
        {
            Console.WriteLine("Сервер закрыл соединение.");
            return;
        }

        if (answer == "OK")
        {
            nickAccepted = true;
        }
        else
        {
            // ответ вида "ERR причина" - показываем причину без "ERR "
            Console.WriteLine(answer.Replace("ERR ", ""));
        }
    }

    Console.WriteLine("Подключено! Команды: /list - кто в чате, /w ник текст - личное сообщение, /exit - выход.");

    // ---------- Приём сообщений ----------
    // Сообщения от сервера могут прийти в любой момент, даже пока мы печатаем.
    // Поэтому читаем их в отдельной задаче, параллельно с вводом с клавиатуры.
    Task receiveTask = Task.Run(async () =>
    {
        try
        {
            string? line = await reader.ReadLineAsync();
            while (line != null)
            {
                Console.WriteLine(line);
                line = await reader.ReadLineAsync();
            }
        }
        catch (Exception)
        {
            // соединение оборвалось - сообщим ниже
        }
        Console.WriteLine("Соединение с сервером закрыто. Нажмите Enter для выхода.");
    });

    // ---------- Отправка сообщений ----------
    while (true)
    {
        string? message = Console.ReadLine();

        // null - ввод закончился; receiveTask завершена - сервера больше нет
        if (message == null || receiveTask.IsCompleted)
        {
            break;
        }
        if (message.Trim() == "")
        {
            continue; // пустые сообщения не отправляем
        }

        await writer.WriteLineAsync(message);

        if (message == "/exit")
        {
            break;
        }
    }
}
catch (IOException)
{
    Console.WriteLine("Связь с сервером потеряна.");
}
finally
{
    client.Close();
}
