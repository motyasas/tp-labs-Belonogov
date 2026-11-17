using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    /// <summary>
    /// Сервер чата.
    /// Протокол простой: одно сообщение = одна строка текста.
    ///   1) Клиент первой строкой присылает ник.
    ///      Сервер отвечает "OK" (ник принят) или "ERR текст" (ник не подходит, надо прислать другой).
    ///   2) Дальше каждая строка от клиента - это сообщение или команда:
    ///      /list           - список пользователей онлайн
    ///      /w ник текст    - личное сообщение
    ///      /exit           - выход
    /// </summary>
    public class Server
    {
        private const int HistorySize = 10;      // сколько последних сообщений помнить (бонус)
        private const string LogFile = "chat.log";

        private readonly int _port;

        // Подключённые клиенты: ник -> "писатель" в его сетевой поток.
        // Dictionary удобен тем, что по нику сразу находим, кому писать (для /w).
        private readonly Dictionary<string, StreamWriter> _clients = new Dictionary<string, StreamWriter>();

        // Последние сообщения чата - их отправляем новичку (бонус "история чата")
        private readonly Queue<string> _history = new Queue<string>();

        // Каждый клиент обслуживается своей задачей, все они работают одновременно
        // и обращаются к _clients и _history. Поэтому доступ к ним - только под lock.
        private readonly object _clientsLock = new object();

        // Отдельный замок для файла журнала: два потока не должны писать в файл одновременно
        private readonly object _logLock = new object();

        public Server(int port)
        {
            _port = port;
        }

        /// <summary>
        /// Запуск сервера: бесконечно принимаем новых клиентов.
        /// </summary>
        public async Task RunAsync()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, _port);
            listener.Start();
            Log($"Сервер запущен на порту {_port}");

            while (true)
            {
                // Ждём нового клиента. await не блокирует поток, пока никто не подключается.
                TcpClient client = await listener.AcceptTcpClientAsync();

                // Запускаем обработку клиента и НЕ ждём её окончания (нет await),
                // сразу возвращаемся к приёму следующего клиента.
                // "_ =" просто говорит компилятору: "результат задачи нам не нужен".
                _ = HandleClientAsync(client);
            }
        }

        /// <summary>
        /// Обслуживание одного клиента от подключения до отключения.
        /// </summary>
        private async Task HandleClientAsync(TcpClient client)
        {
            string endpoint = client.Client.RemoteEndPoint?.ToString() ?? "?";
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);
            writer.AutoFlush = true; // отправлять каждую строку сразу, а не копить в буфере

            string? nick = null;

            // try/catch: если клиент внезапно закроет окно, чтение выбросит исключение.
            // Ловим его здесь, чтобы упал только этот клиент, а не весь сервер.
            try
            {
                nick = await RegisterNickAsync(reader, writer);
                if (nick == null)
                {
                    return; // клиент отключился, так и не выбрав ник
                }

                Log($"{nick} подключился ({endpoint})");
                SendHistory(writer);
                Broadcast($"*** {nick} вошёл в чат ***");

                // Основной цикл: читаем строки, пока клиент не отключится
                string? line = await reader.ReadLineAsync();
                while (line != null && line != "/exit")
                {
                    ProcessLine(nick, writer, line);
                    line = await reader.ReadLineAsync();
                }
            }
            catch (Exception ex)
            {
                // Обычно это IOException - соединение разорвано
                Log($"Обрыв соединения {nick ?? endpoint}: {ex.Message}");
            }
            finally
            {
                // finally выполняется всегда: и при нормальном выходе, и после ошибки
                client.Close();

                if (nick != null)
                {
                    lock (_clientsLock)
                    {
                        _clients.Remove(nick);
                    }
                    Log($"{nick} отключился");
                    Broadcast($"*** {nick} покинул чат ***");
                }
            }
        }

        /// <summary>
        /// Читает ник, пока клиент не пришлёт свободный.
        /// Возвращает принятый ник или null, если клиент отключился.
        /// </summary>
        private async Task<string?> RegisterNickAsync(StreamReader reader, StreamWriter writer)
        {
            while (true)
            {
                string? nick = await reader.ReadLineAsync();
                if (nick == null)
                {
                    return null;
                }

                nick = nick.Trim();

                if (nick == "" || nick.Contains(' '))
                {
                    Send(writer, "ERR Ник не может быть пустым и не должен содержать пробелов.");
                    continue; // ждём следующую попытку
                }

                bool added = false;
                lock (_clientsLock)
                {
                    // Проверка и добавление - под одним lock, иначе два клиента
                    // с одинаковым ником могли бы проскочить одновременно
                    if (!_clients.ContainsKey(nick))
                    {
                        _clients.Add(nick, writer);
                        added = true;
                    }
                }

                if (added)
                {
                    Send(writer, "OK");
                    return nick;
                }

                Send(writer, $"ERR Ник «{nick}» уже занят, выберите другой.");
            }
        }

        /// <summary>
        /// Обработка одной строки от клиента: команда или обычное сообщение.
        /// </summary>
        private void ProcessLine(string nick, StreamWriter writer, string line)
        {
            if (line == "/list")
            {
                Send(writer, "Сейчас в чате: " + string.Join(", ", GetNicks()));
                return;
            }

            if (line.StartsWith("/w "))
            {
                SendPrivate(nick, writer, line);
                return;
            }

            string message = $"[{DateTime.Now:HH:mm:ss}] {nick}: {line}";
            Log($"{nick}: {line}");
            AddToHistory(message);
            Broadcast(message);
        }

        /// <summary>
        /// Личное сообщение. Формат строки: "/w ник текст".
        /// </summary>
        private void SendPrivate(string fromNick, StreamWriter fromWriter, string line)
        {
            // Split(' ', 3) режет строку максимум на 3 части:
            // "/w Вася привет как дела" -> ["/w", "Вася", "привет как дела"]
            string[] parts = line.Split(' ', 3);
            if (parts.Length < 3 || parts[2].Trim() == "")
            {
                Send(fromWriter, "ERR Формат: /w ник текст");
                return;
            }

            string toNick = parts[1];
            string text = parts[2];

            StreamWriter? toWriter = null;
            lock (_clientsLock)
            {
                if (_clients.ContainsKey(toNick))
                {
                    toWriter = _clients[toNick];
                }
            }

            if (toWriter == null)
            {
                Send(fromWriter, $"ERR Пользователь «{toNick}» не найден.");
                return;
            }

            string time = DateTime.Now.ToString("HH:mm:ss");
            Send(toWriter, $"[{time}] (лично от {fromNick}): {text}");
            Send(fromWriter, $"[{time}] (лично для {toNick}): {text}");
            Log($"{fromNick} -> {toNick} (лично): {text}");
        }

        // ---------------- Вспомогательные методы ----------------

        /// <summary>
        /// Отправка одной строки одному клиенту.
        /// StreamWriter нельзя использовать из двух потоков сразу, а писать одному клиенту
        /// могут одновременно несколько задач (например, двое других клиентов шлют сообщения).
        /// Поэтому запись в конкретный writer тоже делаем под lock (замок - сам writer).
        /// </summary>
        private void Send(StreamWriter writer, string message)
        {
            try
            {
                lock (writer)
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception)
            {
                // Клиент уже отключился. Ничего страшного: его задача сама удалит его из списка.
            }
        }

        /// <summary>
        /// Рассылка сообщения всем подключённым клиентам.
        /// </summary>
        private void Broadcast(string message)
        {
            // Делаем копию списка под lock и рассылаем уже по копии:
            // так мы не держим lock долго, пока идёт отправка по сети.
            List<StreamWriter> snapshot;
            lock (_clientsLock)
            {
                snapshot = new List<StreamWriter>(_clients.Values);
            }

            foreach (StreamWriter writer in snapshot)
            {
                Send(writer, message);
            }
        }

        private List<string> GetNicks()
        {
            lock (_clientsLock)
            {
                return new List<string>(_clients.Keys);
            }
        }

        private void AddToHistory(string message)
        {
            lock (_clientsLock)
            {
                _history.Enqueue(message);
                if (_history.Count > HistorySize)
                {
                    _history.Dequeue(); // выкидываем самое старое
                }
            }
        }

        private void SendHistory(StreamWriter writer)
        {
            List<string> copy;
            lock (_clientsLock)
            {
                copy = new List<string>(_history);
            }

            if (copy.Count == 0)
            {
                return;
            }

            Send(writer, $"--- Последние сообщения ({copy.Count}) ---");
            foreach (string message in copy)
            {
                Send(writer, message);
            }
            Send(writer, "--- Конец истории ---");
        }

        /// <summary>
        /// Пишет событие на экран сервера и дозаписывает в файл chat.log.
        /// </summary>
        private void Log(string text)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}";
            Console.WriteLine(line);

            try
            {
                lock (_logLock)
                {
                    // AppendAllText дописывает в конец файла (а не перезаписывает его)
                    File.AppendAllText(LogFile, line + Environment.NewLine);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Не удалось записать в журнал: " + ex.Message);
            }
        }
    }
}
