namespace BankSystem.Core;

/// <summary>
/// Реализация №1: журнал хранится в памяти (в обычном списке).
/// Удобна для тестов - не создаёт никаких файлов. После закрытия программы записи пропадают.
/// </summary>
public class MemoryTransactionLog : ITransactionLog
{
    private readonly List<string> _records = new List<string>();

    public void Write(string record)
    {
        _records.Add(record);
    }

    public List<string> ReadAll()
    {
        // Возвращаем копию, чтобы снаружи нельзя было изменить наш внутренний список
        return new List<string>(_records);
    }
}
