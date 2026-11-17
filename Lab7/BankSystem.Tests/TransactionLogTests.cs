using BankSystem.Core;

namespace BankSystem.Tests;

public class TransactionLogTests
{
    [Fact]
    public void MemoryLog_WriteTwoRecords_ReadAllReturnsThemInOrder()
    {
        // Arrange
        MemoryTransactionLog log = new MemoryTransactionLog();

        // Act
        log.Write("первая");
        log.Write("вторая");

        // Assert
        Assert.Equal(new List<string> { "первая", "вторая" }, log.ReadAll());
    }

    [Fact]
    public void MemoryLog_New_IsEmpty()
    {
        MemoryTransactionLog log = new MemoryTransactionLog();

        Assert.Empty(log.ReadAll());
    }

    [Fact]
    public void FileLog_WriteRecords_SavesThemToFile()
    {
        // Arrange: уникальное имя временного файла, чтобы тесты не мешали друг другу
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        FileTransactionLog log = new FileTransactionLog(path);

        try
        {
            // Act
            log.Write("запись 1");
            log.Write("запись 2");

            // Assert
            Assert.Equal(new List<string> { "запись 1", "запись 2" }, log.ReadAll());
            Assert.True(File.Exists(path));
        }
        finally
        {
            File.Delete(path); // убираем за собой
        }
    }

    [Fact]
    public void FileLog_FileDoesNotExist_ReadAllReturnsEmpty()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        FileTransactionLog log = new FileTransactionLog(path);

        Assert.Empty(log.ReadAll());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FileLog_EmptyPath_Throws(string path)
    {
        Assert.Throws<ArgumentException>(() => new FileTransactionLog(path));
    }
}
