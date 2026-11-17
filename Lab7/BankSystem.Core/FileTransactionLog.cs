namespace BankSystem.Core;

/// <summary>
/// Реализация №2: журнал хранится в текстовом файле, одна запись - одна строка.
/// Записи сохраняются между запусками программы.
/// </summary>
public class FileTransactionLog : ITransactionLog
{
    private readonly string _filePath;

    public FileTransactionLog(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Путь к файлу журнала не может быть пустым.");
        }
        _filePath = filePath;
    }

    public void Write(string record)
    {
        // AppendAllText дописывает строку в конец файла (файл создаётся, если его нет)
        File.AppendAllText(_filePath, record + Environment.NewLine);
    }

    public List<string> ReadAll()
    {
        if (!File.Exists(_filePath))
        {
            return new List<string>(); // журнала ещё нет - значит, записей нет
        }
        return new List<string>(File.ReadAllLines(_filePath));
    }
}
